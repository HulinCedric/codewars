using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Xunit;
using Xunit.Sdk;

namespace Codewars.CreateASimpleClassAtRuntime;

public class SolutionTest
{
    [Fact]
    public void BasicTest()
    {
        var rand = new Random();
        var myType = typeof(object);
        Dictionary<string, Type> properties;

        // Define first class
        properties = new Dictionary<string, Type>
            { { "SomeInt", typeof(int) }, { "SomeString", typeof(string) }, { "SomeObject", typeof(object) } };
        Kata.DefineClass("SomeClass", properties, ref myType);
        // Instantiate first class
        var myInstance = CreateInstance(myType);
        myInstance.SomeObject = myInstance;
        myInstance.SomeString = "Hey there";
        myInstance.SomeInt = 3;
        Console.WriteLine($"{myInstance.SomeObject}: {myInstance.SomeString}, {myInstance.SomeInt}");

        // Define second class
        properties = new Dictionary<string, Type>
        {
            { "AnotherObject", typeof(object) }, { "SomeDouble", typeof(double) },
            { "AnotherString", typeof(string) }
        };
        Kata.DefineClass("AnotherClass_N" + rand.Next(100), properties, ref myType);

        // Instantiate second class
        myInstance = CreateInstance(myType);
        myInstance.AnotherObject = "User: ";
        myInstance.AnotherString = "My lucky number is ";
        myInstance.SomeDouble = 92835768;

        Console.WriteLine($"{myInstance.AnotherObject}: {myInstance.AnotherString} {myInstance.SomeDouble} ");

        // Try to redefine first class
        if (Kata.DefineClass("SomeClass", properties, ref myType))
            throw new XunitException("This class is already defined");
    }

    private dynamic CreateInstance(Type myType)
        => Assembly.GetAssembly(myType).CreateInstance(myType.Name);
}

public static class Kata
{
    private const string AssemblyName = "RuntimeAssembly";

    private static readonly ModuleBuilder ModuleBuilder;

    static Kata()
    {
        var assemblyName = new AssemblyName(AssemblyName);

        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            assemblyName,
            AssemblyBuilderAccess.Run);

        // The module name is usually the same as the assembly name.
        ModuleBuilder = assemblyBuilder.DefineDynamicModule(AssemblyName);
    }

    public static bool DefineClass(string className, Dictionary<string, Type> properties, ref Type actualType)
    {
        if (IsTypeAlreadyDefined(className))
            return false;

        var typeBuilder = DefineType(className);

        foreach (var (propertyName, propertyType) in properties)
            typeBuilder.DefineAutoImplementedProperty(propertyName, propertyType);

        actualType = typeBuilder.CreateType()!;

        return true;
    }

    private static TypeBuilder DefineType(string className)
        => ModuleBuilder.DefineType(
            className,
            TypeAttributes.Public);

    private static bool IsTypeAlreadyDefined(string className)
        => ModuleBuilder.GetType(className) is not null;
}

internal static class AutoImplementedPropertyDefinitionExtensions
{
    // The property "set" and property "get" methods require a special set of attributes.
    private const MethodAttributes GetterAndSetterAttributes = MethodAttributes.Public |
                                                               MethodAttributes.SpecialName |
                                                               MethodAttributes.HideBySig;

    internal static void DefineAutoImplementedProperty(
        this TypeBuilder typeBuilder,
        string propertyName,
        Type propertyType)
    {
        var fieldBuilder = DefinePropertyField(typeBuilder, propertyName, propertyType);
        var getterBuilder = DefinePropertyGetter(typeBuilder, propertyName, propertyType, fieldBuilder);
        var setterBuilder = DefinePropertySetter(typeBuilder, propertyName, propertyType, fieldBuilder);

        DefineProperty(typeBuilder, propertyName, propertyType, getterBuilder, setterBuilder);
    }

    private static void DefineProperty(
        TypeBuilder typeBuilder,
        string propertyName,
        Type propertyType,
        MethodBuilder getterBuilder,
        MethodBuilder setterBuilder)
    {
        // Define a property that gets and sets the private field.
        //
        // The last argument of DefineProperty is null, because the
        // property has no parameters. (If you don't specify null, you must
        // specify an array of Type objects. For a parameterless property,
        // use the built-in array with no elements: Type.EmptyTypes)
        var propertyBuilder = typeBuilder.DefineProperty(
            propertyName,
            PropertyAttributes.HasDefault,
            propertyType,
            null);

        // Map the "get" and "set" accessor methods to the PropertyBuilder.
        // The property is now complete.
        propertyBuilder.SetGetMethod(getterBuilder);
        propertyBuilder.SetSetMethod(setterBuilder);
    }

    private static MethodBuilder DefinePropertySetter(
        TypeBuilder typeBuilder,
        string propertyName,
        Type propertyType,
        FieldInfo fieldInfo)
    {
        // Define the "set" accessor method for the property,
        // which has no return type and takes one argument of type propertyType.
        var setterBuilder = typeBuilder.DefineMethod(
            $"set_{propertyName}",
            GetterAndSetterAttributes,
            null,
            new[] { propertyType });

        var setterGenerator = setterBuilder.GetILGenerator();

        // Load the instance and then the value argument,
        // then store the argument in the field.
        setterGenerator.Emit(OpCodes.Ldarg_0);
        setterGenerator.Emit(OpCodes.Ldarg_1);
        setterGenerator.Emit(OpCodes.Stfld, fieldInfo);
        setterGenerator.Emit(OpCodes.Ret);

        return setterBuilder;
    }

    private static MethodBuilder DefinePropertyGetter(
        TypeBuilder typeBuilder,
        string propertyName,
        Type propertyType,
        FieldInfo fieldInfo)
    {
        // Define the "get" accessor method for a property.
        // The method returns a type and has no arguments.
        // (Note that null could be used instead of Types.EmptyTypes)
        var getterBuilder = typeBuilder.DefineMethod(
            $"get_{propertyName}",
            GetterAndSetterAttributes,
            propertyType,
            Type.EmptyTypes);

        var getterGenerator = getterBuilder.GetILGenerator();

        // For an instance property, argument zero is the instance.
        // Load the instance, then load the private field and return,
        // leaving the field value on the stack.
        getterGenerator.Emit(OpCodes.Ldarg_0);
        getterGenerator.Emit(OpCodes.Ldfld, fieldInfo);
        getterGenerator.Emit(OpCodes.Ret);

        return getterBuilder;
    }

    private static FieldBuilder DefinePropertyField(
            TypeBuilder typeBuilder,
            string propertyName,
            Type propertyType)
        // Add a private field.
        => typeBuilder.DefineField(
            $"m_{propertyName.ToLowerInvariant()}",
            propertyType,
            FieldAttributes.Private);
}