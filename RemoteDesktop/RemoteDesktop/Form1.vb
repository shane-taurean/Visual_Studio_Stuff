Imports MSTSCLib

Public Class Form1

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If Button1.Text = "Connect" Then
            If txtServer.Text = Nothing Or txtUser.Text = Nothing Or txtPassword.Text = Nothing Then
                MessageBox.Show("Enter the following information:" & vbCrLf & _
                                "Server Name, Username and Password", "Error!", _
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation)

            Else
                Try
                    rdp.Server = txtServer.Text
                    rdp.UserName = txtUser.Text
                    Dim secured As IMsTscNonScriptable = DirectCast(rdp.GetOcx(), IMsTscNonScriptable)
                    secured.ClearTextPassword = txtPassword.Text
                    rdp.Connect()
                    Button1.Text = "Disconnect"

                Catch ex As Exception
                    MessageBox.Show(ex.ToString)
                End Try
            End If
        ElseIf Button1.Text = "Disconnect" Then
            Try
                If (rdp.Connected.ToString() = "1") Then
                    rdp.Disconnect()
                End If
                Button1.Text = "Connect"
            Catch ex As Exception
                MessageBox.Show(ex.ToString)
            End Try
        End If
    End Sub

    Private Sub Form1_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            If (rdp.Connected.ToString() = "1") Then
                rdp.Disconnect()
            End If
        Catch ex As Exception
            'MessageBox.Show(ex.ToString)
        End Try
    End Sub
End Class
