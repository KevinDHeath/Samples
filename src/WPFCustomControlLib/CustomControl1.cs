using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace WPFCustomControlLib;

public class CustomControl1 : TextBox
{
	static CustomControl1()
	{
		DefaultStyleKeyProperty.OverrideMetadata( typeof( CustomControl1 ),
			new FrameworkPropertyMetadata( typeof( CustomControl1 ) ) );
	}

	private Button? _btnClick = null;

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		_btnClick = GetTemplateChild( "btnClick" ) as Button;
		if( _btnClick is not null )
		{
			_btnClick.Click += btnClick_OnClick;
		}
	}

	private int counter;
	[SuppressMessage( "Style", "IDE1006:Naming Styles", Justification = "Control name" )]
	private void btnClick_OnClick( object sender, RoutedEventArgs e )
	{
		counter++;
		Text = counter.ToString();
	}
}