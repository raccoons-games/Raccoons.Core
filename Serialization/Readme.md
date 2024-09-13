# Raccoons Serialization

### JsonUtilitySerializer

Create JsonUtilitySerializer-object, and use it as ISerializer implementation instead of straight JsonUtility-usage.

**Methods**:

- string Serialize(object obj) - serialize objects into json-string.
- T Deserialize(string str) - deserialize objects from json-string.

### ISerializer

Base interface for implementation of serialization. Implement your custom serializer/deserializer and use it as ISerializer.