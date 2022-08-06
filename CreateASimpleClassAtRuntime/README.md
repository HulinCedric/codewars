# Create A Class at Runtime

Your method will accept as arguments a string containing the **class name**, a dictionary String -> Type containing
the **properties**, and a ref to the actual **Type of the class** after it has been created.  
You should check if a class already exists in the same assembly and return false if so, also you should make sure to
create every class in the same assembly, let's call it "**RuntimeAssembly**", namespace is optional, but the class names
will be passed to your method without any namespace.

The properties of each of your classes will then be accessed and modified normally, e.g:

```csharp
properties = new Dictionary<string, Type> { { "AString", typeof(string) } };    
Kata.DefineClass("SimpleClass", properties, ref myType);    
myInstance = CreateInstance(myType);    
myInstance.AString = "Hi";   
```

You will pass the kata if none of these operations throw an exception, and if the values are actually changed.

Happy coding
