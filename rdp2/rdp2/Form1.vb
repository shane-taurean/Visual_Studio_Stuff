Public Class Form1

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If Button1.Text = "Connect" Then
            Try
                If ((txtServerName.Text <> Nothing) Or (txtUsername.Text <> Nothing) Or (txtPassword.Text <> Nothing)) Then
                    rdpClient.Server = txtServerName.Text
                    rdpClient.UserName = txtUsername.Text
                    rdpClient.AdvancedSettings8.ClearTextPassword = txtPassword.Text
                    rdpClient.ColorDepth = 32
                    rdpClient.DesktopWidth = 800
                    rdpClient.DesktopHeight = 600
                    ' rdpClient.FullScreen = True
                    rdpClient.AdvancedSettings8.SmartSizing = True

                    rdpClient.Connect()

                End If
            Catch ex As Exception
                MessageBox.Show("1 " + ex.ToString)
            End Try
        ElseIf Button1.Text = "Disconnect" Then
            Try
                If rdpClient.Connected = 1 Then
                    rdpClient.Disconnect()
                End If

            Catch ex As Exception
                MessageBox.Show("2 " + ex.ToString)
            End Try

        End If

    End Sub

    Private Sub rdpClient_connected(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rdpClient.OnConnected
        Button1.Text = "Disconnect"
    End Sub

    Private Sub rdpClient_Disconnected(ByVal sender As System.Object, ByVal e As AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEvent) Handles rdpClient.OnDisconnected
        Button1.Text = "Connect"
        rdpClient.DisconnectedText = "disconnected"
    End Sub



    Private Sub Form1_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            If rdpClient.Connected = 1 Then
                rdpClient.Disconnect()
            End If

        Catch ex As Exception
            MessageBox.Show("3 " + ex.ToString)
        End Try
    End Sub
End Class
