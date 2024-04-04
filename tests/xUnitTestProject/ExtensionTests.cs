using System.Net;
using System.Security;

using ClassLibrary.Extensions;

namespace xUnitTestProject;

public class ExtensionTests
{
	[Fact]
	public void ByteArray_should_have_length_of_12()
	{
		// Arrange
		byte[] array = [0x00, 0x21, 0x60, 0x1F, 0xA1, 0x07];

		// Act
		string result = array.ToHexString();

		// Assert
		result.Length.Should().Be( 12 );
	}

	[Fact]
	public void SecureString_should_be_equal()
	{
		// Arrange
		SecureString s1 = new NetworkCredential( "", "hello" ).SecurePassword;
		SecureString s2 = new NetworkCredential( "", "hello" ).SecurePassword;

		// Act
		bool result = s1.IsEqualTo( s2 );

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void SecureString_should_not_be_equal()
	{
		// Arrange
		SecureString s1 = new NetworkCredential( "", "hello" ).SecurePassword;
		SecureString s2 = new NetworkCredential( "", "not-equal" ).SecurePassword;

		// Act
		bool result = s1.IsEqualTo( s2 );

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void ComparePasswords_should_be_false()
	{
		// Arrange
		SecureString s1 = new NetworkCredential( "", "hello" ).SecurePassword;
		SecureString s2 = new NetworkCredential( "", "not-equal" ).SecurePassword;

		// Act
		bool result = SecureStringEx.IsEqual( s1, s2 );

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void ComparePasswords_should_be_true()
	{
		// Arrange
		SecureString s1 = new NetworkCredential( "", "hello" ).SecurePassword;
		SecureString s2 = new NetworkCredential( "", "hello" ).SecurePassword;

		// Act
		bool result = SecureStringEx.IsEqual( s1, s2 );

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void SecureStringToString_should_not_be_null()
	{
		// Arrange
		SecureString s1 = new NetworkCredential( "", "hello" ).SecurePassword;

		// Act
		string? result = SecureStringEx.SecureStringToString( s1 );

		// Assert
		result.Should().NotBeNull();
	}
}