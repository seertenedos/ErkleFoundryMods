using System;
using System.Collections.Generic;

namespace Expressive.Expressions.Binary.Bitwise
{
    internal class BitwiseAndExpression : BinaryExpressionBase
    {
        #region Constructors

        public BitwiseAndExpression(IExpression lhs, IExpression rhs, Context context) : base(lhs, rhs, context)
        {
        }

        #endregion

        #region BinaryExpressionBase Members

        /// <inheritdoc />
        protected override object EvaluateImpl(object lhsResult, IExpression rightHandSide, IDictionary<string, object> variables) =>
            EvaluateAggregates(lhsResult, rightHandSide, variables, (l, r) => Convert.ToUInt16(l, System.Globalization.CultureInfo.InvariantCulture) & Convert.ToUInt16(r, System.Globalization.CultureInfo.InvariantCulture));

        #endregion
    }
}
