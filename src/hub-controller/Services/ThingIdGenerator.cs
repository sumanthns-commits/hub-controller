using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Services
{
    public class ThingIdGenerator : IThingIdGenerator
    {
        public string Generate()
        {
            var length = Constants.THING_ID_LENGTH;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }
    }
}
