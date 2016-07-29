using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ContentManagementBackend.Demo
{
    public static class BootstrapHelper
    {

        //методы
        public static MvcHtmlString BootstrapDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper
            , Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> items
            , string buttonText = null)
        {
            var member = expression.Body as MemberExpression;
            if (member == null)
            {
                string parameterName = nameof(expression);
                string message = string.Format("The parameter {0} must be a member accessing labda such as x => x.Id"
                    , parameterName);
                throw new ArgumentException(message, parameterName);
            }

            buttonText = buttonText ?? member.Member.Name;
            string buttonID = "dropdownMenu1";

            StringBuilder builder = new StringBuilder();
            builder.Append("<div class=\"dropdown\">");
            builder.AppendFormat("<button class=\"btn btn-default dropdown-toggle\" type=\"button\" id=\"{0}\" data-toggle=\"dropdown\">{1}< span class=\"caret\"></span></button>"
                , buttonID, buttonText);
            builder.AppendFormat("<ul class=\"dropdown-menu\" role=\"menu\" aria-labelledby=\"{0}\">"
                , buttonID);

            foreach (SelectListItem item in items)
            {
                builder.AppendFormat("<li role=\"presentation\" data-value=\"{0}\"><a role=\"menuitem\" tabindex=\"- 1\" href=\"#\">{1}</a></li>"
                    , item.Value, item.Text);
            }

            builder.Append("</ul>");
            builder.Append("</div>");

            return new MvcHtmlString(builder.ToString());
        }
    }
}