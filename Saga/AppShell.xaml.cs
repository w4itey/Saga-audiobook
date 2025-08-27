namespace Saga;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Debug to confirm AppShell is being created
		System.Diagnostics.Debug.WriteLine("AppShell constructor called");
	}
}
