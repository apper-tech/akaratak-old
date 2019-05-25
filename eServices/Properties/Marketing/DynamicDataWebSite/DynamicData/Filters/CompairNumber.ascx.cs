using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Web.DynamicData;
using System.Web.UI;
using System.Web.UI.WebControls;
using NotAClue.Web.DynamicData;
using NotAClue.Web;
using System.Globalization;
using DynamicDataLibrary.Attributes;

namespace DynamicDataWebSite
{
    public partial class CompairNumberFilter : System.Web.DynamicData.QueryableFilterUserControl
    {
        public string DateFrom
        {
            get { return txbNumber.Text; }
        }

        public override Control FilterControl
        {
            get { return txbNumber; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            txbNumber.ToolTip = this.AppRelativeVirtualPath.GetFileNameTitle();

        }


        public override IQueryable GetQueryable(IQueryable source)
        {
            if (String.IsNullOrEmpty(DateFrom))
                return source;

            ParameterExpression parameterExpression = Expression.Parameter(source.ElementType, "item");
            Expression body = BuildQueryBody(parameterExpression);

            LambdaExpression lambda = Expression.Lambda(body, parameterExpression);
            MethodCallExpression whereCall = Expression.Call(typeof(Queryable), "Where", new Type[] { source.ElementType }, source.Expression, Expression.Quote(lambda));
            return source.Provider.CreateQuery(whereCall);
        }

        private Expression BuildQueryBody(ParameterExpression parameterExpression)
        {
            Expression propertyExpression = LinqExpressionHelper.GetValue(LinqExpressionHelper.CreatePropertyExpression(parameterExpression, Column.Name));
            TypeConverter converter = TypeDescriptor.GetConverter(Column.ColumnType);



            BinaryExpressionAttribute hint = (BinaryExpressionAttribute)this.Column.Attributes[typeof(BinaryExpressionAttribute)];
            Func<Expression, Expression, BinaryExpression> binaryExpression = BinaryExpression.Equal;
            if (hint != null)
            {
                binaryExpression = hint.BinaryExpressionFunction;
            }

            BinaryExpression greaterThanExpression = BuildCompareExpression(propertyExpression,
                converter.ConvertFromString(DateFrom), binaryExpression);
            return greaterThanExpression;
        }

        private BinaryExpression BuildCompareExpression(Expression propertyExpression, object value, Func<Expression, Expression, BinaryExpression> comparisonFunction)
        {
            ConstantExpression valueExpression = Expression.Constant(value);
            BinaryExpression comparison = comparisonFunction(propertyExpression, valueExpression);
            return comparison;
        }

        protected void btnRangeButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            if (button.ID == btnClear.ID)
                txbNumber.Text = String.Empty;
            else
                txbNumber.Text = txbNumber.Text.Trim();

            OnFilterChanged();
        }
    }
}