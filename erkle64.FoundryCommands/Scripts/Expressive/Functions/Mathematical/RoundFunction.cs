using Expressive.Expressions;
using System;

namespace Expressive.Functions.Mathematical
{
    internal class RoundFunction : FunctionBase
    {
        #region FunctionBase Members

        public override string Name { get { return "Round"; } }

        public override object Evaluate(IExpression[] parameters, Context context)
        {
            this.ValidateParameterCount(parameters, 2, 2);

            return Math.Round(Convert.ToDouble(parameters[0].Evaluate(Variables), System.Globalization.CultureInfo.InvariantCulture), Convert.ToInt32(parameters[1].Evaluate(Variables), System.Globalization.CultureInfo.InvariantCulture));
        }

        #endregion
    }
}
