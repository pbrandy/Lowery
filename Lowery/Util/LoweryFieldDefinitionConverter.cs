using Lowery.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lowery.Util
{
    internal class LoweryFieldDefinitionConverter : JsonConverter<LoweryFieldDefinition>
    {
        public override LoweryFieldDefinition? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new LoweryFieldDefinition();
        }

        public override void Write(Utf8JsonWriter writer, LoweryFieldDefinition value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
