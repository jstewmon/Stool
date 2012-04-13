﻿
using System.Web;
using Newtonsoft.Json;

namespace Stool
{
    public static class Extensions
    {
        public static void Send<T>(this HttpContext context, T data)
        {
            var serializer = new JsonSerializer();
            context.Response.ContentType = "application/json";
            serializer.Serialize(context.Response.Output, data);
        }
    }
}