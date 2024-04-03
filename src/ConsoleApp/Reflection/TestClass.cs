using ClassLibrary.Models;

namespace ConsoleApp.Reflection;

// Define an example interface.
public interface ITestClass { }

// Define a class that meets the constraints on the type parameter of class TestClass.
public class TestClass : Movie, ITestClass
{
	public TestClass() { }
}

// Define a generic class with one parameter. The parameter has three constraints:
// It must inherit Movie, it must implement ITestClass, and it must have a parameterless constructor.
public class TestClass<T> where T : Movie, ITestClass, new() { }