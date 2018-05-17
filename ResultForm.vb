

Public Class ResultForm
    Public Sub New(ByVal resultText As String)
        InitializeComponent()
        TextBox1.Text = resultText
    End Sub
    Public Sub New()
        InitializeComponent()
        TextBox1.Text = "No Results."
    End Sub
End Class