Public Class Form1

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim sum As Int32 = 0
        Dim cdigit As Char
        Dim i, digit As Int32
        Dim nDigit As Int32
        Dim parity As Int32
        'Dim digit As Double

        nDigit = TextBox1.TextLength
        'MessageBox.Show(nDigit.ToString)
        If nDigit > 0 Then
            parity = nDigit Mod 2

            For i = 0 To nDigit - 1
                cdigit = TextBox1.Text.Chars(i)
                digit = Char.GetNumericValue(cdigit)

                If i Mod 2 = parity Then
                    digit = digit * 2
                End If

                If digit > 9 Then
                    digit = digit - 9
                End If

                sum = sum + digit
            Next

            If ((sum <> 0) And (sum Mod 10 = 0)) Then
                'MsgBox("Approved")
                PictureBox1.Image = My.Resources._true
            Else
                'MsgBox("Not Approved")
                PictureBox1.Image = My.Resources._false
            End If
        End If
        
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        PictureBox1.Image = My.Resources.unknown
    End Sub

    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        PictureBox1.Image = My.Resources.unknown
    End Sub
End Class
