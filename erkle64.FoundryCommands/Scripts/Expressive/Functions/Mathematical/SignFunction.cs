using Expressive.Expressions;
using Expressive.Helpers;
using System;

namespace Expressive.Functions.Mathematical
{
    internal class SignFunction : FunctionBase
    {
        #region FunctionBase Members

        public override string Name { get { return "Sign"; } }

        public override object Evaluate(IExpression[] parameters, Context context)
        {
            this.ValidateParameterCount(parameters, 1, 1);

            var value = parameters[0].Evaluate(Variables);

            if (value != null)
            {
                var valueType = TypeHelper.GetTypeCode(value);

                switch (valueType)
                {
                    case TypeCode.Decimal:
                        return Math.Sign(Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture));
                    case TypeCode.Double:
                        return Math.Sign(Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture));
                    case TypeCode.Int16:
                        return Math.Sign(Convert.ToInt16(value, System.Globalization.CultureInfo.InvariantCulture));
                    case TypeCode.UInt16:
                        return Math.Sign(Convert.ToUInt16(value, System.Globalization.CultureInfo.InvariantCulture));
                    case TypeCode.Int32:
                        return Math.Sign(Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture));
                    case TypeCode.UInt32:
                        return Math.Sign(Convert.ToUInt32(value, System.Globalization.CultureInfo.InvariantCulture));
                    case TypeCode.Int64:
                        return Math.Sign(Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture));
                    case TypeCode.SByte:
                        return Math.Sign(Convert.ToSByte(value, System.Globalization.CultureInfo.InvariantCulture));
                    case TypeCode.Single:
                        return Math.Sign(Convert.ToSingle(value, System.Globalization.CultureInfo.InvariantCulture));
                    default:
                        break;
                }
            }

            return null;
        }

        #endregion
    }
}
