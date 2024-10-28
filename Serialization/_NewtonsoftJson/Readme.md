# Raccoons Newtonsoft Json Serialization

### NewtonsoftJsonSerializer

Create NewtonsoftJsonSerializer-object, and use it as ISerializer implementation instead of straight JsonConvert-usage.

**Methods**:

- string Serialize(object obj) - serialize objects into json-string.
- T Deserialize(string str) - deserialize objects from json-string.
