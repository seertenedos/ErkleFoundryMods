using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;


#if ENABLE_IL2CPP
using UnityEngine.Scripting;
#endif


namespace TinyJSON
{
	/// <summary>
	/// Mark members that should be included.
	/// Public fields are included by default.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public sealed class Include : Attribute {}


	/// <summary>
	/// Mark members that should be excluded.
	/// Private fields and all properties are excluded by default.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class Exclude : Attribute {}


	/// <summary>
	/// Mark methods to be called after an object is decoded.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method )]
	public class AfterDecode : Attribute {}


	/// <summary>
	/// Mark methods to be called before an object is encoded.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method )]
	public class BeforeEncode : Attribute {}


	/// <summary>
	/// Mark members to force type hinting even when EncodeOptions.NoTypeHints is set.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class TypeHint : Attribute {}


	/// <summary>
	/// Provide field and property aliases when an object is decoded.
	/// If a field or property is not found while decoding, this list will be searched for a matching alias.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true )]
	public class DecodeAlias : Attribute
	{
		public string[] Names { get; private set; }


		public DecodeAlias( params string[] names )
		{
			Names = names;
		}


		public bool Contains( string name )
		{
			return Array.IndexOf( Names, name ) > -1;
		}
	}


	[Obsolete( "Use the Exclude attribute instead." )]
	// ReSharper disable once UnusedMember.Global
	public sealed class Skip : Exclude {}


	[Obsolete( "Use the AfterDecode attribute instead." )]
	// ReSharper disable once UnusedMember.Global
	public sealed class Load : AfterDecode {}


	public sealed class DecodeException : Exception
	{
		public DecodeException( string message )
			: base( message ) {}


		public DecodeException( string message, Exception innerException )
			: base( message, innerException ) {}
	}


#if ENABLE_IL2CPP
	[Preserve]
#endif
	// ReSharper disable once InconsistentNaming
	public static class JSON
	{
		static readonly Type includeAttrType = typeof(Include);
		static readonly Type excludeAttrType = typeof(Exclude);
		static readonly Type decodeAliasAttrType = typeof(DecodeAlias);


		public static Variant Load( string json )
		{
			if (json == null)
			{
				throw new ArgumentNullException( "json" );
			}

			return Decoder.Decode( json );
		}


		public static string Dump( object data )
		{
			return Dump( data, EncodeOptions.None );
		}


		public static string Dump( object data, EncodeOptions options )
		{
			// Invoke methods tagged with [BeforeEncode] attribute.
			if (data != null)
			{
				var type = data.GetType();
				if (!(type.IsEnum || type.IsPrimitive || type.IsArray))
				{
					foreach (var method in type.GetMethods( instanceBindingFlags ))
					{
						if (method.GetCustomAttributes( false ).AnyOfType( typeof(BeforeEncode) ))
						{
							if (method.GetParameters().Length == 0)
							{
								method.Invoke( data, null );
							}
						}
					}
				}
			}

			return Encoder.Encode( data, options );
		}


		public static void MakeInto<T>( Variant data, out T item )
		{
			item = DecodeType<T>( data );
		}

		public static void PopulateInto<T>( Variant data, ref T item, Dictionary<Type, PopulateOverride> overrides)
		{
			PopulateType(ref item, data, overrides);
		}

		public static void PopulateInto<T>( Variant data, ref T item, Dictionary<Type, PopulateOverride> overrides, PopulateExpression populateExpression)
		{
            PopulateTypeWithExpressions(ref item, data, overrides, populateExpression);
		}


		static readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();

		static Type FindType( string fullName )
		{
			if (fullName == null)
			{
				return null;
			}

            if (typeCache.TryGetValue(fullName, out Type type))
            {
                return type;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				type = assembly.GetType( fullName );
				if (type != null)
				{
					typeCache.Add( fullName, type );
					return type;
				}
			}

			return null;
		}


#if ENABLE_IL2CPP
		[Preserve]
#endif
		static T DecodeType<T>( Variant data )
		{
			if (data == null)
			{
				return default(T);
			}

			var type = typeof(T);

			if (type.IsEnum)
			{
				return (T) Enum.Parse( type, data.ToString( CultureInfo.InvariantCulture ) );
			}

			if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
			{
				return (T) Convert.ChangeType( data, type );
			}

			if (type == typeof(Guid))
			{
				return (T) (object) new Guid( data.ToString( CultureInfo.InvariantCulture ) );
			}

			if (type.IsArray)
			{
				if (type.GetArrayRank() == 1)
				{
					var makeFunc = decodeArrayMethod.MakeGenericMethod( type.GetElementType() );
					return (T) makeFunc.Invoke( null, new object[] { data } );
				}

                if (!(data is ProxyArray arrayData))
                {
                    throw new DecodeException("Variant is expected to be a ProxyArray here, but it is not.");
                }

                var arrayRank = type.GetArrayRank();
				var rankLengths = new int[arrayRank];
				if (arrayData.CanBeMultiRankArray( rankLengths ))
				{
					var elementType = type.GetElementType();
					if (elementType == null)
					{
						throw new DecodeException( "Array element type is expected to be not null, but it is." );
					}

					var array = Array.CreateInstance( elementType, rankLengths );
					var makeFunc = decodeMultiRankArrayMethod.MakeGenericMethod( elementType );
					try
					{
						makeFunc.Invoke( null, new object[] { arrayData, array, 1, rankLengths } );
					}
					catch (Exception e)
					{
						throw new DecodeException( "Error decoding multidimensional array. Did you try to decode into an array of incompatible rank or element type?", e );
					}

					return (T) Convert.ChangeType( array, typeof(T) );
				}

				throw new DecodeException( "Error decoding multidimensional array; JSON data doesn't seem fit this structure." );
			}

			if (typeof(IList).IsAssignableFrom( type ))
			{
				var makeFunc = decodeListMethod.MakeGenericMethod( type.GetGenericArguments() );
				return (T) makeFunc.Invoke( null, new object[] { data } );
			}

			if (typeof(IDictionary).IsAssignableFrom( type ))
			{
				var makeFunc = decodeDictionaryMethod.MakeGenericMethod( type.GetGenericArguments() );
				return (T) makeFunc.Invoke( null, new object[] { data } );
			}

			// At this point we should be dealing with a class or struct.
			T instance;
            if (!(data is ProxyObject proxyObject))
            {
                throw new InvalidCastException("ProxyObject expected when decoding into '" + type.FullName + "'.");
            }

            // If there's a type hint, use it to create the instance.
            var typeHint = proxyObject.TypeHint;
			if (typeHint != null && typeHint != type.FullName)
			{
				var makeType = FindType( typeHint );
				if (makeType == null)
				{
					throw new TypeLoadException( "Could not load type '" + typeHint + "'." );
				}

				if (type.IsAssignableFrom( makeType ))
				{
					instance = (T) Activator.CreateInstance( makeType );
					type = makeType;
				}
				else
				{
					throw new InvalidCastException( "Cannot assign type '" + typeHint + "' to type '" + type.FullName + "'." );
				}
			}
			else
			{
				// We don't have a type hint, so just instantiate the type we have.
				instance = Activator.CreateInstance<T>();
			}


			// Now decode fields and properties.
			foreach (var pair in (ProxyObject) data)
			{
				var field = type.GetField( pair.Key, instanceBindingFlags );

				// If the field doesn't exist, search through any [DecodeAlias] attributes.
				if (field == null)
				{
					var fields = type.GetFields( instanceBindingFlags );
					foreach (var fieldInfo in fields)
					{
						foreach (var attribute in fieldInfo.GetCustomAttributes( true ))
						{
							if (decodeAliasAttrType.IsInstanceOfType( attribute ))
							{
								if (((DecodeAlias) attribute).Contains( pair.Key ))
								{
									field = fieldInfo;
									break;
								}
							}
						}
					}
				}

				if (field != null)
				{
					var shouldDecode = field.IsPublic;
					foreach (var attribute in field.GetCustomAttributes( true ))
					{
						if (excludeAttrType.IsInstanceOfType( attribute ))
						{
							shouldDecode = false;
						}

						if (includeAttrType.IsInstanceOfType( attribute ))
						{
							shouldDecode = true;
						}
					}

					if (shouldDecode)
					{
						var makeFunc = decodeTypeMethod.MakeGenericMethod( field.FieldType );
						if (type.IsValueType)
						{
							// Type is a struct.
							var instanceRef = (object) instance;
							field.SetValue( instanceRef, makeFunc.Invoke( null, new object[] { pair.Value } ) );
							instance = (T) instanceRef;
						}
						else
						{
							// Type is a class.
							field.SetValue( instance, makeFunc.Invoke( null, new object[] { pair.Value } ) );
						}
					}
				}

				var property = type.GetProperty( pair.Key, instanceBindingFlags );

				// If the property doesn't exist, search through any [DecodeAlias] attributes.
				if (property == null)
				{
					var properties = type.GetProperties( instanceBindingFlags );
					foreach (var propertyInfo in properties)
					{
						foreach (var attribute in propertyInfo.GetCustomAttributes( false ))
						{
							if (decodeAliasAttrType.IsInstanceOfType( attribute ))
							{
								if (((DecodeAlias) attribute).Contains( pair.Key ))
								{
									property = propertyInfo;
									break;
								}
							}
						}
					}
				}

				if (property != null)
				{
					if (property.CanWrite && property.GetCustomAttributes( false ).AnyOfType( includeAttrType ))
					{
						var makeFunc = decodeTypeMethod.MakeGenericMethod( new Type[] { property.PropertyType } );
						if (type.IsValueType)
						{
							// Type is a struct.
							var instanceRef = (object) instance;
							property.SetValue( instanceRef, makeFunc.Invoke( null, new object[] { pair.Value } ), null );
							instance = (T) instanceRef;
						}
						else
						{
							// Type is a class.
							property.SetValue( instance, makeFunc.Invoke( null, new object[] { pair.Value } ), null );
						}
					}
				}
			}

			// Invoke methods tagged with [AfterDecode] attribute.
			foreach (var method in type.GetMethods( instanceBindingFlags ))
			{
				if (method.GetCustomAttributes( false ).AnyOfType( typeof(AfterDecode) ))
				{
					method.Invoke( instance, method.GetParameters().Length == 0 ? null : new object[] { data } );
				}
			}

			return instance;
		}

        public delegate object PopulateOverride(Variant input, object original);
        public delegate object PopulateExpression(string source, object original);

#if ENABLE_IL2CPP
		[Preserve]
#endif
		static void PopulateType<T>(ref T objectToPopulate, Variant data, Dictionary<Type, PopulateOverride> overrides)
		{
            PopulateTypeWithExpressions(ref objectToPopulate, data, overrides, null);
		}

#if ENABLE_IL2CPP
		[Preserve]
#endif
        static void PopulateTypeWithExpressions<T>(ref T objectToPopulate, Variant data, Dictionary<Type, PopulateOverride> overrides, PopulateExpression populateExpression)
		{
			if (data == null)
			{
				return;
			}

			var type = typeof(T);

			if (overrides.TryGetValue(type, out var overrideFunc))
            {
                objectToPopulate = (T)overrideFunc(data, objectToPopulate);
                return;
            }

			if (type == typeof(Vector3Int) && data is ProxyObject vectorObject)
			{
				var vector = new Vector3Int();
				vector.x = vectorObject["x"].ToInt32(CultureInfo.InvariantCulture);
				vector.y = vectorObject["y"].ToInt32(CultureInfo.InvariantCulture);
				vector.z = vectorObject["z"].ToInt32(CultureInfo.InvariantCulture);
				objectToPopulate = (T)(object)vector;
			}

			if (type.IsEnum)
			{
                objectToPopulate = (T)Enum.Parse( type, data.ToString( CultureInfo.InvariantCulture ) );
				return;
			}

			if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
			{
				if (data is ProxyString proxyString)
				{
					var dataString = proxyString.ToString();
					if (dataString.StartsWith("{") && dataString.EndsWith("}"))
					{
						if (populateExpression != null)
						{
							objectToPopulate = (T)Convert.ChangeType(populateExpression?.Invoke(dataString.Substring(1, dataString.Length - 2), objectToPopulate), type);
                        }
                    }
                    else
					{
						objectToPopulate = (T)Convert.ChangeType(data, type);
					}
				}
				else
				{
					objectToPopulate = (T)Convert.ChangeType(data, type);
				}
				return;
			}

			if (type == typeof(Guid))
			{
                objectToPopulate = (T)(object)new Guid( data.ToString( CultureInfo.InvariantCulture ) );
				return;
			}

			if (type.IsArray)
			{
				if (type.GetArrayRank() == 1)
				{
					var makeFunc = populateArrayMethod.MakeGenericMethod( type.GetElementType() );
                    var arguments = new object[] { objectToPopulate, data, overrides, populateExpression };
                    makeFunc.Invoke( null, arguments);
                    objectToPopulate = (T)arguments[0];
					return;
				}

                if (!(data is ProxyArray arrayData))
                {
                    throw new DecodeException("Variant is expected to be a ProxyArray here, but it is not.");
                }

                var arrayRank = type.GetArrayRank();
				var rankLengths = new int[arrayRank];
				if (arrayData.CanBeMultiRankArray( rankLengths ))
				{
					var elementType = type.GetElementType();
					if (elementType == null)
					{
						throw new DecodeException( "Array element type is expected to be not null, but it is." );
					}

					var array = Array.CreateInstance( elementType, rankLengths );
					var makeFunc = populateMultiRankArrayMethod.MakeGenericMethod( elementType );
					try
					{
						makeFunc.Invoke( null, new object[] { arrayData, array, 1, rankLengths, overrides, populateExpression } );
					}
					catch (Exception e)
					{
						throw new DecodeException( "Error decoding multidimensional array. Did you try to decode into an array of incompatible rank or element type?", e );
					}

                    objectToPopulate = (T) Convert.ChangeType( array, typeof(T) );
					return;
				}

				throw new DecodeException( "Error decoding multidimensional array; JSON data doesn't seem fit this structure." );
			}

			if (typeof(IList).IsAssignableFrom( type ))
			{
				var makeFunc = populateListMethod.MakeGenericMethod( type.GetGenericArguments() );
                var arguments = new object[] { objectToPopulate, data, overrides, populateExpression };
                makeFunc.Invoke(null, arguments);
                objectToPopulate = (T)arguments[0];
                return;
			}

			if (typeof(IDictionary).IsAssignableFrom( type ))
			{
				var makeFunc = populateDictionaryMethod.MakeGenericMethod( type.GetGenericArguments() );
                var arguments = new object[] { objectToPopulate, data, overrides, populateExpression };
                makeFunc.Invoke(null, arguments);
                objectToPopulate = (T)arguments[0];
                return;
			}

            // At this point we should be dealing with a class or struct.
            if (!(data is ProxyObject))
            {
                throw new InvalidCastException("ProxyObject expected when decoding into '" + type.FullName + "'.");
            }

            // Now decode fields and properties.
            foreach (var pair in (ProxyObject) data)
			{
				var field = type.GetField( pair.Key, instanceBindingFlags );

				// If the field doesn't exist, search through any [DecodeAlias] attributes.
				if (field == null)
				{
					var fields = type.GetFields( instanceBindingFlags );
					foreach (var fieldInfo in fields)
					{
						foreach (var attribute in fieldInfo.GetCustomAttributes( true ))
						{
							if (decodeAliasAttrType.IsInstanceOfType( attribute ))
							{
								if (((DecodeAlias) attribute).Contains( pair.Key ))
								{
									field = fieldInfo;
									break;
								}
							}
						}
					}
				}

				if (field != null)
				{
					var shouldDecode = field.IsPublic;
					foreach (var attribute in field.GetCustomAttributes( true ))
					{
						if (excludeAttrType.IsInstanceOfType( attribute ))
						{
							shouldDecode = false;
						}

						if (includeAttrType.IsInstanceOfType( attribute ))
						{
							shouldDecode = true;
						}
					}

					if (shouldDecode)
					{
						var makeFunc = populateTypeMethod.MakeGenericMethod( field.FieldType );
						if (type.IsValueType)
						{
							// Type is a struct.
							var instanceRef = (object) objectToPopulate;
                            var arguments = new object[] { field.GetValue(instanceRef), pair.Value, overrides, populateExpression };
                            makeFunc.Invoke(null, arguments);
                            field.SetValue( instanceRef, arguments[0]);
                            objectToPopulate = (T) instanceRef;
						}
						else
						{
							// Type is a class.
                            var arguments = new object[] { field.GetValue(objectToPopulate), pair.Value, overrides, populateExpression };
                            makeFunc.Invoke(null, arguments);
                            field.SetValue(objectToPopulate, arguments[0]);
                        }
                    }
				}

				var property = type.GetProperty( pair.Key, instanceBindingFlags );

				// If the property doesn't exist, search through any [DecodeAlias] attributes.
				if (property == null)
				{
					var properties = type.GetProperties( instanceBindingFlags );
					foreach (var propertyInfo in properties)
					{
						foreach (var attribute in propertyInfo.GetCustomAttributes( false ))
						{
							if (decodeAliasAttrType.IsInstanceOfType( attribute ))
							{
								if (((DecodeAlias) attribute).Contains( pair.Key ))
								{
									property = propertyInfo;
									break;
								}
							}
						}
					}
				}

				if (property != null)
				{
					if (property.CanWrite && property.GetCustomAttributes( false ).AnyOfType( includeAttrType ))
					{
						var makeFunc = populateTypeMethod.MakeGenericMethod( new Type[] { property.PropertyType } );
						if (type.IsValueType)
						{
							// Type is a struct.
							var instanceRef = (object)objectToPopulate;
                            var arguments = new object[] { property.GetValue(instanceRef, null), pair.Value, overrides, populateExpression };
                            makeFunc.Invoke(null, arguments);
                            property.SetValue( instanceRef, arguments[0], null );
                            objectToPopulate = (T) instanceRef;
						}
						else
						{
                            // Type is a class.
                            var arguments = new object[] { property.GetValue(objectToPopulate, null), pair.Value, overrides, populateExpression };
                            makeFunc.Invoke(null, arguments);
                            property.SetValue(objectToPopulate, arguments[0], null );
						}
					}
				}
			}

			// Invoke methods tagged with [AfterDecode] attribute.
			foreach (var method in type.GetMethods( instanceBindingFlags ))
			{
				if (method.GetCustomAttributes( false ).AnyOfType( typeof(AfterDecode) ))
				{
					method.Invoke(objectToPopulate, method.GetParameters().Length == 0 ? null : new object[] { data } );
				}
			}
		}


#if ENABLE_IL2CPP
		[Preserve]
#endif
		// ReSharper disable once UnusedMethodReturnValue.Local
		static List<T> DecodeList<T>( Variant data )
		{
			var list = new List<T>();

            if (!(data is ProxyArray proxyArray))
            {
                throw new DecodeException("Variant is expected to be a ProxyArray here, but it is not.");
            }

            foreach (var item in proxyArray)
			{
				list.Add( DecodeType<T>( item ) );
			}

			return list;
		}


#if ENABLE_IL2CPP
		[Preserve]
#endif
		// ReSharper disable once UnusedMethodReturnValue.Local
		static void PopulateList<T>(ref List<T> list, Variant data, Dictionary<Type, PopulateOverride> overrides, PopulateExpression populateExpression)
		{
			if (list == null) list = new List<T>();
			else list.Clear();
            if (!(data is ProxyArray proxyArray))
            {
                throw new DecodeException("Variant is expected to be a ProxyArray here, but it is not.");
            }

            foreach (var item in proxyArray)
			{
				T value = default(T);
                PopulateTypeWithExpressions(ref value, item, overrides, populateExpression);
                list.Add(value);
			}
		}


#if ENABLE_IL2CPP
		[Preserve]
#endif
		// ReSharper disable once UnusedMethodReturnValue.Local
		static Dictionary<TKey, TValue> DecodeDictionary<TKey, TValue>( Variant data )
		{
			var dict = new Dictionary<TKey, TValue>();
			var type = typeof(TKey);

            if (!(data is ProxyObject proxyObject))
            {
                throw new DecodeException("Variant is expected to be a ProxyObject here, but it is not.");
            }

            foreach (var pair in proxyObject)
			{
				var k = (TKey) (type.IsEnum ? Enum.Parse( type, pair.Key ) : Convert.ChangeType( pair.Key, type ));
				var v = DecodeType<TValue>( pair.Value );
				dict.Add( k, v );
			}

			return dict;
		}


#if ENABLE_IL2CPP
		[Preserve]
#endif
		// ReSharper disable once UnusedMethodReturnValue.Local
		static void PopulateDictionary<TKey, TValue>(ref Dictionary<TKey, TValue> dict, Variant data, Dictionary<Type, PopulateOverride> overrides, PopulateExpression populateExpression)
		{
			if (dict == null) dict = new Dictionary<TKey, TValue>();
			else dict.Clear();
			var type = typeof(TKey);

            if (!(data is ProxyObject proxyObject))
            {
                throw new DecodeException("Variant is expected to be a ProxyObject here, but it is not.");
            }

            foreach (var pair in proxyObject)
			{
				var k = (TKey) (type.IsEnum ? Enum.Parse( type, pair.Key ) : Convert.ChangeType( pair.Key, type ));
				var v = default(TValue);
                PopulateTypeWithExpressions(ref v, pair.Value, overrides, populateExpression);
				dict[k] = v;
			}
		}


#if ENABLE_IL2CPP
		[Preserve]
#endif
		// ReSharper disable once UnusedMethodReturnValue.Local
		static T[] DecodeArray<T>( Variant data )
		{
            if (!(data is ProxyArray arrayData))
            {
                throw new DecodeException("Variant is expected to be a ProxyArray here, but it is not.");
            }

            var arraySize = arrayData.Count;
			var array = new T[arraySize];

			var i = 0;
			foreach (var item in arrayData)
			{
				array[i++] = DecodeType<T>( item );
			}

			return array;
		}


#if ENABLE_IL2CPP
		[Preserve]
#endif
		// ReSharper disable once UnusedMethodReturnValue.Local
		static void PopulateArray<T>(ref T[] array, Variant data, Dictionary<Type, PopulateOverride> overrides, PopulateExpression populateExpression)
		{
            if (!(data is ProxyArray arrayData))
            {
                throw new DecodeException("Variant is expected to be a ProxyArray here, but it is not.");
            }

            var arraySize = arrayData.Count;
			array = new T[arraySize];

			var i = 0;
			foreach (var item in arrayData)
			{
                var value = default(T);
                PopulateTypeWithExpressions(ref value, item, overrides, populateExpression);
				array[i++] = value;
			}
		}


#if ENABLE_IL2CPP
		[Preserve]
#endif
		// ReSharper disable once UnusedMember.Local
		static void DecodeMultiRankArray<T>( ProxyArray arrayData, Array array, int arrayRank, int[] indices )
		{
			var count = arrayData.Count;
			for (var i = 0; i < count; i++)
			{
				indices[arrayRank - 1] = i;
				if (arrayRank < array.Rank)
				{
					DecodeMultiRankArray<T>( arrayData[i] as ProxyArray, array, arrayRank + 1, indices );
				}
				else
				{
					array.SetValue( DecodeType<T>( arrayData[i] ), indices );
				}
			}
		}


#if ENABLE_IL2CPP
		[Preserve]
#endif
		// ReSharper disable once UnusedMember.Local
		static void PopulateMultiRankArray<T>( ProxyArray arrayData, Array array, int arrayRank, int[] indices, Dictionary<Type, PopulateOverride> overrides, PopulateExpression populateExpression)
		{
			var count = arrayData.Count;
			for (var i = 0; i < count; i++)
			{
				indices[arrayRank - 1] = i;
				if (arrayRank < array.Rank)
				{
                    PopulateMultiRankArray<T>( arrayData[i] as ProxyArray, array, arrayRank + 1, indices, overrides, populateExpression );
				}
				else
				{
					var value = default(T);
					PopulateTypeWithExpressions(ref value, arrayData[i], overrides, populateExpression);
                    array.SetValue(value , indices );
				}
			}
		}


		const BindingFlags instanceBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		const BindingFlags staticBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
		static readonly MethodInfo decodeTypeMethod = typeof(JSON).GetMethod( "DecodeType", staticBindingFlags );
		static readonly MethodInfo decodeListMethod = typeof(JSON).GetMethod( "DecodeList", staticBindingFlags );
		static readonly MethodInfo decodeDictionaryMethod = typeof(JSON).GetMethod( "DecodeDictionary", staticBindingFlags );
		static readonly MethodInfo decodeArrayMethod = typeof(JSON).GetMethod( "DecodeArray", staticBindingFlags );
		static readonly MethodInfo decodeMultiRankArrayMethod = typeof(JSON).GetMethod( "DecodeMultiRankArray", staticBindingFlags );
		static readonly MethodInfo populateTypeMethod = typeof(JSON).GetMethod("PopulateTypeWithExpressions", staticBindingFlags );
		static readonly MethodInfo populateListMethod = typeof(JSON).GetMethod("PopulateList", staticBindingFlags );
		static readonly MethodInfo populateDictionaryMethod = typeof(JSON).GetMethod("PopulateDictionary", staticBindingFlags );
		static readonly MethodInfo populateArrayMethod = typeof(JSON).GetMethod("PopulateArray", staticBindingFlags );
		static readonly MethodInfo populateMultiRankArrayMethod = typeof(JSON).GetMethod("PopulateMultiRankArray", staticBindingFlags );


#if ENABLE_IL2CPP
		[Preserve]
#endif
		// ReSharper disable once InconsistentNaming
		public static void SupportTypeForAOT<T>()
		{
			DecodeType<T>( null );
			DecodeList<T>( null );
			DecodeArray<T>( null );
			DecodeDictionary<Int16, T>( null );
			DecodeDictionary<UInt16, T>( null );
			DecodeDictionary<Int32, T>( null );
			DecodeDictionary<UInt32, T>( null );
			DecodeDictionary<Int64, T>( null );
			DecodeDictionary<UInt64, T>( null );
			DecodeDictionary<Single, T>( null );
			DecodeDictionary<Double, T>( null );
			DecodeDictionary<Decimal, T>( null );
			DecodeDictionary<Boolean, T>( null );
			DecodeDictionary<String, T>( null );
		}


#if ENABLE_IL2CPP
		[Preserve]
#endif
		// ReSharper disable once InconsistentNaming
		// ReSharper disable once UnusedMember.Local
		static void SupportValueTypesForAOT()
		{
			SupportTypeForAOT<Int16>();
			SupportTypeForAOT<UInt16>();
			SupportTypeForAOT<Int32>();
			SupportTypeForAOT<UInt32>();
			SupportTypeForAOT<Int64>();
			SupportTypeForAOT<UInt64>();
			SupportTypeForAOT<Single>();
			SupportTypeForAOT<Double>();
			SupportTypeForAOT<Decimal>();
			SupportTypeForAOT<Boolean>();
			SupportTypeForAOT<String>();
		}
	}
}
