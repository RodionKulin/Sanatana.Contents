using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ContentManagementBackend.Demo
{
    public class ObjectIdModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            string value = bindingContext.ModelType == typeof(List<ObjectId>)
                ? ReadValue(controllerContext, bindingContext.ModelName + "[]")
                : ReadValue(controllerContext, bindingContext.ModelName);

            if (String.IsNullOrEmpty(value))
            {
                return null;
            }

            if (bindingContext.ModelType == typeof(List<ObjectId>))
            {
                return ParseList(value);
            }
            else
            {
                return ParseSingle(value);
            }
        }
        private string ReadValue(ControllerContext controllerContext, string modelName)
        {
            string value = controllerContext.RouteData.Values[modelName] as string;
            value = value ?? controllerContext.RequestContext.HttpContext.Request.QueryString[modelName] as string;
            value = value ?? controllerContext.RequestContext.HttpContext.Request.Form[modelName] as string;

            return value;
        }
        private object ParseList(string value)
        {
            string[] strings = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            List<ObjectId> items = new List<ObjectId>();

            foreach (string str in strings)
            {
                ObjectId objectIdValue;
                if (ObjectId.TryParse(str, out objectIdValue))
                {
                    items.Add(objectIdValue);
                }
            }

            return items;
        }
        private object ParseSingle(string value)
        {
            ObjectId objectIdValue;
            if (ObjectId.TryParse(value, out objectIdValue))
            {
                return objectIdValue;
            }

            return null;
        }
    }
}