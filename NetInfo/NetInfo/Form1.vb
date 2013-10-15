Imports System.Net.NetworkInformation
Imports System
Imports System.Text
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

Public Class Form1
    Shared i As Integer
    Public Ycount As Boolean = False
    Public Xcount As Boolean = False
    Public RealTimeCount As Boolean = False
    Public YPreviousValue As Integer = 0
    Public YCurrentValue As Integer = 0
    Public XPreviousValue As Integer = 0
    Public XCurrentValue As Integer = 0
    Shared reader As String
    'Dim sClient As Socket = New Socket(AddressFamily.InterNetwork, _
    '                                      SocketType.Stream, _
    '                                      ProtocolType.Tcp)
    Private sClient As Socket
    Private recvBuffer(8191) As Byte
    Public Shared ConnectDone As ManualResetEvent = New ManualResetEvent(False)
    Public Shared SendDone As ManualResetEvent = New ManualResetEvent(False)
    Public Shared ReceiveDone As ManualResetEvent = New ManualResetEvent(False)

    Public Delegate Sub DelegateChange(ByVal e As NetworkAvailabilityEventArgs)
    Public Delegate Sub Delegateaddress(ByVal sender As System.Object, ByVal e As System.EventArgs)
    Public Delegate Sub keyDetectiondelegate(ByVal KeyCode As Integer, ByVal Shift As Integer)
    Public Delegate Sub connectedDelegate(ByVal remoteendpoint As String)
    Public Delegate Sub SentDelegate(ByVal bytes As Integer)

    Private Delegate Sub SimpleCallback() 'ByVal RemoteEndpoint As String)
    Private Delegate Sub DisplayTextCallback(ByVal Text As String)

    Public Event insert()


    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Button7.Enabled = False
        RealTimeButton.Enabled = False
        AddHandler NetworkChange.NetworkAvailabilityChanged, AddressOf NetworkAvailablityChanged
        AddHandler NetworkChange.NetworkAddressChanged, AddressOf NetworkAddressChanged

        TrackBar1.Value = 0
        TrackBar2.Value = 0
        InsertItems(Nothing, Nothing)
    End Sub

    Private Sub NetworkAvailablityChanged(ByVal sender As Object, ByVal e As NetworkAvailabilityEventArgs)
        Me.Invoke(New DelegateChange(AddressOf Me.change), e)
    End Sub

    Private Sub NetworkAddressChanged(ByVal sender As Object, ByVal e As EventArgs)
        Me.Invoke(New Delegateaddress(AddressOf TabPage2_entered), Nothing, Nothing)
    End Sub
    'Network Availability changed
    Public Sub change(ByVal e As NetworkAvailabilityEventArgs)
        If e.IsAvailable Then
            ToolStripStatusLabel1.Text = "Network Available"
            TabPage2_entered(Nothing, Nothing)
        Else
            ToolStripStatusLabel1.Text = "Network Unavailable"
            TabPage2_entered(Nothing, Nothing)
        End If
    End Sub
    'Network Info
    Private Sub TabPage2_entered(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TabPage2.Enter
        ListBox1.Items.Clear()
        If NetworkInterface.GetIsNetworkAvailable Then
            Dim interfaces As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces
            For Each ni As NetworkInterface In interfaces
                ListBox1.Items.Add("Network Name: " & ni.Name)
                ListBox1.Items.Add("Description: " & ni.Description)
                ListBox1.Items.Add("Network ID: " & ni.Id)
                ListBox1.Items.Add("Interface Type: " & ni.NetworkInterfaceType.ToString)
                ListBox1.Items.Add("Speed:" & ni.Speed)
                ListBox1.Items.Add("Operational Status: " & ni.OperationalStatus.ToString)
                ListBox1.Items.Add("Physical Address: " & ni.GetPhysicalAddress().ToString)
                ListBox1.Items.Add("Bytes Sent: " & ni.GetIPv4Statistics().BytesSent)
                ListBox1.Items.Add("Bytes Received: " & ni.GetIPv4Statistics().BytesReceived)
                For Each addr As UnicastIPAddressInformation In ni.GetIPProperties.UnicastAddresses
                    ListBox1.Items.Add("IP Address:" & addr.Address.ToString)
                    ListBox1.Items.Add("Lease expires: " & DateTime.Now.AddSeconds(addr.DhcpLeaseLifetime))
                Next
                ListBox1.Items.Add("")
            Next
        Else
            ListBox1.Items.Add("No Network Available")
        End If
    End Sub

    'Ping section
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim comp As String = TextBox1.Text
        ListBox2.Items.Clear()
        ListBox2.Items.Add("Pinging: " & comp)
        Dim png As New Ping
        Try
            '  Ping the specified computer with a time-out of 2000ms.
            Dim reply As PingReply = png.Send(comp, 2000)
            If reply.Status = IPStatus.Success Then
                ListBox2.Items.Add("Success - IP Address: " & reply.Address.ToString)
                ListBox2.Items.Add("Reply Time: " & reply.RoundtripTime.ToString & "ms")
            Else
                ListBox2.Items.Add(reply.Status.ToString)
            End If
        Catch ex As Exception
            If TextBox1.Text = Nothing Then
                ListBox2.Items.Clear()
                ListBox2.Items.Add("No IP address has been entered")
            Else
                Dim err As String = ex.InnerException.Message.ToString
                ListBox2.Items.Add("Error: " & err)
            End If
            Return
        End Try
    End Sub
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''KEY DETECTION'''''''''''''''''''''''''

    'Detect if user pressed Return Key to initiate pinging
    Private Sub TextBox1IPaddress_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles TextBox1.KeyPress
        If e.KeyChar = Chr(13) Then 'Chr(13) is the Enter Key
            'Runs the Button1Ping_Click Event
            Button1_Click(Me, EventArgs.Empty)
        End If
    End Sub

    'Detect if Arrow keys were pressed on the main tabpage
    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, _
 ByVal keyData As Keys) As Boolean
        Dim bhandled As Boolean = False
        Select Case keyData
            Case Keys.Up
                'do whatever....up arrow pressed
                If (TabPage2.Focus = True) Or (TabPage3.Focus = True) Then
                    If TabPage3.Focus = True Then
                        TextBox1.Focus()
                        TextBox1.SelectionStart = TextBox1.TextLength
                    End If
                    'Do Nothing
                Else
                    Try
                        If TrackBar1.Value < TrackBar1.Maximum Then
                            TrackBar1.Value += 1
                        End If
                        'MessageBox.Show("UP")
                    Catch ex As Exception
                        MessageBox.Show(ex.ToString)
                    End Try
                End If
                bhandled = True
            Case Keys.Down
                'do whatever....down arrow pressed
                If (TabPage2.Focus = True) Or (TabPage3.Focus = True) Then
                    If TabPage3.Focus = True Then
                        TextBox1.Focus()
                        TextBox1.SelectionStart = TextBox1.TextLength
                    End If
                    'Do Nothing
                Else
                    Try
                        If TrackBar1.Value > TrackBar1.Minimum Then
                            TrackBar1.Value -= 1
                        End If
                        'MessageBox.Show("DOWN")
                    Catch ex As Exception
                        MessageBox.Show(ex.ToString)
                    End Try
                End If
                bhandled = True
            Case Keys.Left
                'do whatever....Left arrow pressed
                If (TabPage2.Focus = True) Or (TabPage3.Focus = True) Then
                    If TabPage3.Focus = True Then
                        TextBox1.Focus()
                        TextBox1.SelectionStart = TextBox1.TextLength
                    End If
                    'Do Nothing
                Else
                    Try
                        If TrackBar2.Value > TrackBar2.Minimum Then
                            TrackBar2.Value -= 1
                        End If
                        'MessageBox.Show("LEFT")
                    Catch ex As Exception
                        MessageBox.Show(ex.ToString)
                    End Try
                End If
                bhandled = True
            Case Keys.Right
                'do whatever....Right arrow pressed
                If (TabPage2.Focus = True) Or (TabPage3.Focus = True) Then
                    If TabPage3.Focus = True Then
                        TextBox1.Focus()
                        TextBox1.SelectionStart = TextBox1.TextLength
                    End If
                    'Do Nothing
                Else
                    Try
                        If TrackBar2.Value < TrackBar2.Maximum Then
                            TrackBar2.Value += 1
                        End If
                        'MessageBox.Show("right")
                    Catch ex As Exception
                        MessageBox.Show(ex.ToString)
                    End Try
                End If
                bhandled = True
            Case Keys.Add
                If (TabPage2.Focus = True) Or (TabPage3.Focus = True) Then
                    If TabPage3.Focus = True Then
                        TextBox1.Text = TextBox1.Text & "+"
                        TextBox1.Focus()
                        TextBox1.SelectionStart = TextBox1.TextLength

                    End If
                    'Do Nothing
                Else
                    'RaiseEvent Click()
                    InsertItems(Button3, Nothing)
                    bhandled = True
                End If
                bhandled = True
            Case Keys.Subtract
                If (TabPage2.Focus = True) Or (TabPage3.Focus = True) Then
                    If TabPage3.Focus = True Then
                        TextBox1.Text = TextBox1.Text & "-"
                        TextBox1.Focus()
                        TextBox1.SelectionStart = TextBox1.TextLength
                    End If
                    'Do Nothing
                Else
                    RemoveItems(Button4, Nothing)
                End If
                bhandled = True
            Case Keys.Enter
                If (TabPage2.Focus = True) Or (TabPage3.Focus = True) Then
                    If TextBox1.Focus = True Then
                        Button1_Click(Me, EventArgs.Empty)
                        TextBox1.Focus()
                        TextBox1.SelectionStart = TextBox1.TextLength
                    End If
                    'Do Nothing
                Else
                    If TextBox2.Focused = True Or TextBox3.Focused = True Then
                        Button2_Click(Button2, EventArgs.Empty)
                    End If
                End If
                bhandled = True
                'Case Else
                '    bhandled = False
        End Select
        ' End If
        Return bhandled
    End Function
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''



    'Y-Axis Values
    Private Sub TrackBar1_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TrackBar1.Scroll, TrackBar1.ValueChanged
        Label7.Text = TrackBar1.Value
        '''''''''''''''''''''''''''''''''''''''
        If RealTimeButton.Checked = True Then

            If YPreviousValue < TrackBar1.Value Then
                reader = "U"
            Else
                reader = "D"
            End If
            SendData(Nothing, Nothing)
        End If
        '''''''''''''''''''''''''''''''''''''''
        If Ycount = False Then
            YPreviousValue = YCurrentValue
            Ycount = True
            RealTimeCount = False
        Else
            YPreviousValue = TrackBar1.Value
            RealTimeCount = True
        End If
    End Sub
    'X-Axis Values
    Private Sub TrackBar2_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TrackBar2.Scroll, TrackBar2.ValueChanged
        Label8.Text = TrackBar2.Value
        '''''''''''''''''''''''''''''''''''''''
        If RealTimeButton.Checked = True Then

            If XPreviousValue < TrackBar2.Value Then
                reader = "R"
            Else
                reader = "L"
            End If
            SendData(Nothing, Nothing)
        End If
        '''''''''''''''''''''''''''''''''''''''
        If Xcount = False Then
            XPreviousValue = XCurrentValue
            Xcount = True
        Else
            XPreviousValue = TrackBar2.Value
        End If
    End Sub
    'Insert Items
    Private Sub InsertItems(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        If RealTimeButton.Checked = False Then
            If Ycount = True Then
                YPreviousValue = YCurrentValue
                Ycount = False
            Else
                YPreviousValue = TrackBar1.Value
                Ycount = True
            End If
            YCurrentValue = TrackBar1.Value

            If Xcount = True Then
                XPreviousValue = XCurrentValue
                Xcount = False
            Else
                XPreviousValue = TrackBar2.Value
                Xcount = True
            End If
            XCurrentValue = TrackBar2.Value

            Label9.Text = YPreviousValue
            Label11.Text = XPreviousValue

            If ListBox3.SelectedIndex <> -1 Then
                If YCurrentValue < YPreviousValue Then
                    For steps As Integer = 1 To (YPreviousValue - YCurrentValue)
                        ListBox3.Items.Insert(ListBox3.SelectedIndex, "D")
                    Next
                ElseIf YCurrentValue > YPreviousValue Then
                    For steps As Integer = 1 To (YCurrentValue - YPreviousValue)
                        ListBox3.Items.Insert(ListBox3.SelectedIndex, "U")
                    Next
                Else
                    ListBox3.Items.Insert(ListBox3.SelectedIndex, "N")
                End If
                If XCurrentValue < XPreviousValue Then
                    For steps As Integer = 1 To (XPreviousValue - XCurrentValue)
                        ListBox3.Items.Insert(ListBox3.SelectedIndex, "L")
                    Next
                ElseIf XCurrentValue > XPreviousValue Then
                    For steps As Integer = 1 To (XCurrentValue - XPreviousValue)
                        ListBox3.Items.Insert(ListBox3.SelectedIndex, "R")
                    Next
                Else
                    ListBox3.Items.Insert(ListBox3.SelectedIndex, "N")
                End If
            Else
                If YCurrentValue < YPreviousValue Then
                    For steps As Integer = 1 To (YPreviousValue - YCurrentValue)
                        ListBox3.Items.Insert(ListBox3.Items.Count, "D")
                    Next
                ElseIf YCurrentValue > YPreviousValue Then
                    For steps As Integer = 1 To (YCurrentValue - YPreviousValue)
                        ListBox3.Items.Insert(ListBox3.Items.Count, "U")
                    Next
                Else
                    ListBox3.Items.Insert(ListBox3.Items.Count, "N")
                End If
                If XCurrentValue < XPreviousValue Then
                    For steps As Integer = 1 To (XPreviousValue - XCurrentValue)
                        ListBox3.Items.Insert(ListBox3.Items.Count, "L")
                    Next
                ElseIf XCurrentValue > XPreviousValue Then
                    For steps As Integer = 1 To (XCurrentValue - XPreviousValue)
                        ListBox3.Items.Insert(ListBox3.Items.Count, "R")
                    Next
                Else
                    ListBox3.Items.Insert(ListBox3.Items.Count, "N")
                End If
            End If
            Dim ItemArray(Me.ListBox3.Items.Count - 1) As Object
            Me.ListBox3.Items.CopyTo(ItemArray, 0)
            Dim xdata As String = Join(ItemArray)
            reader = xdata.Replace(" ", "")
        End If
    End Sub
    'Remove items
    Private Sub RemoveItems(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        'ListBox3.Items.Remove(ListBox3.SelectedItem)
        If RealTimeButton.Checked = False Then
            If ListBox3.SelectedIndex <> -1 Then
                ListBox3.Items.RemoveAt(ListBox3.SelectedIndex)
            Else
                If ListBox3.Items.Count = 0 Then
                    Exit Sub
                Else
                    ListBox3.Items.RemoveAt(ListBox3.Items.Count - 1)
                End If
            End If
            Dim ItemArray(Me.ListBox3.Items.Count - 1) As Object
            Me.ListBox3.Items.CopyTo(ItemArray, 0)
            Dim xdata As String = Join(ItemArray)
            reader = xdata.Replace(" ", "")
        End If
    End Sub


    'Save File
    Private Sub SaveData(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        Dim sfd As SaveFileDialog = New SaveFileDialog()
        sfd.ShowDialog()
        Dim SaveFilename As String = sfd.FileName.ToString
        Dim ItemArray(Me.ListBox3.Items.Count - 1) As Object
        Me.ListBox3.Items.CopyTo(ItemArray, 0)
        Dim Data As String = Join(ItemArray, Environment.NewLine)
        reader = Data.Replace(" ", "")
        Try
            My.Computer.FileSystem.WriteAllText(SaveFilename, Data, False)
        Catch
            Return
        End Try
    End Sub

    'Open File
    Private Sub LoadData(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        ListBox3.Items.Clear()
        Dim ofd As OpenFileDialog = New OpenFileDialog()
        ofd.ShowDialog()
        Dim OpenFilename As String = ofd.FileName.ToString
        Try
            reader = My.Computer.FileSystem.ReadAllText(OpenFilename)
            Dim strs() As String
            strs = Split(reader, Environment.NewLine)
            For Each s As String In strs
                ListBox3.Items.Add(s)
            Next
            Dim ItemArray(Me.ListBox3.Items.Count - 1) As Object
            Me.ListBox3.Items.CopyTo(ItemArray, 0)
            Dim Data As String = Join(ItemArray)
            reader = Data.Replace(" ", "")

        Catch
            Return
        End Try
    End Sub

    'Clear Queue
    Private Sub Button8_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button8.Click
        ListBox3.Items.Clear()
        ListBox3.Items.Insert(ListBox3.Items.Count, "N")
        ListBox3.Items.Insert(ListBox3.Items.Count, "N")
        reader = "N"
    End Sub

    ''''''''''''SEND DATA'''''''''
    'Send Data when operator clicks on the Run button
    Private Sub SendData(ByVal sender As System.Object, ByVal e As System.EventArgs) 'Handles Button7.Click
        Dim byteData As Byte() = Encoding.ASCII.GetBytes(reader)

        sClient.BeginSend(byteData, 0, byteData.Length, 0, _
                      AddressOf SendCallback, Nothing)
    End Sub

    Private Sub SendData2(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button7.Click
        Dim ItemArray(Me.ListBox3.Items.Count - 1) As Object
        Me.ListBox3.Items.CopyTo(ItemArray, 0)
        Dim Data As String = Join(ItemArray)
        reader = Data.Replace(" ", "")
        Dim byteData As Byte() = Encoding.ASCII.GetBytes(reader)

        sClient.BeginSend(byteData, 0, byteData.Length, 0, _
                      AddressOf SendCallback, Nothing)
    End Sub

    Public Sub SendCallback(ByVal ar As IAsyncResult)
        'Dim sClient As Socket = CType(ar.AsyncState, Socket)
        Dim bytesSent As Integer
        Try
            SyncLock sClient
                bytesSent = sClient.EndSend(ar)
            End SyncLock

            BeginInvoke(New SentDelegate(AddressOf sentbytes), bytesSent)
            ConnectDone.Set()
            SendDone.Set()
        Catch ex As Exception
            MessageBox.Show("Error sending data")
        End Try

    End Sub

    Public Sub sentbytes(ByVal bytes As Integer)
        'ListBox4.Items.Add("Sent " & bytes.ToString & " bytes to server.")
        'ListBox4.SelectedIndex = ListBox4.Items.Count - 1
        TextBox4.AppendText("Number of bytes sent to server: " & bytes.ToString & vbCrLf)
        TextBox4.SelectionStart = TextBox4.Text.Length
        TextBox4.AppendText("Data sent to server: " & reader.ToString & vbCrLf)
        TextBox4.SelectionStart = TextBox4.Text.Length
    End Sub

    Private Sub CToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Config.ShowDialog()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        About.ShowDialog()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub

    Private Sub SaveAsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveAsToolStripMenuItem.Click
        If RealTimeButton.Checked = False Then
            SaveData(Nothing, Nothing)
        Else
            MessageBox.Show("This will work only in the Programmable Mode")
        End If
    End Sub

    Private Sub LoadToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LoadToolStripMenuItem.Click
        If RealTimeButton.Checked = False Then
            LoadData(Nothing, Nothing)
        Else
            MessageBox.Show("This will work only in the Programmable Mode")
        End If
    End Sub

    Private Sub PGMButton_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PGMButton.CheckedChanged

        If PGMButton.Checked = True And Button2.Text = "Disconnect" Then
            Button7.Enabled = True
            Dim ItemArray(Me.ListBox3.Items.Count - 1) As Object
            Me.ListBox3.Items.CopyTo(ItemArray, 0)
            Dim Data As String = Join(ItemArray)
            Try
                reader = Data.Replace(" ", "")
            Catch ex As Exception
            End Try

        Else
            Button7.Enabled = False
        End If
    End Sub

    Private Sub RealTimeButton_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RealTimeButton.CheckedChanged
        If RealTimeButton.Checked = True And Button2.Text = "Disconnect" Then
            Button7.Enabled = False
            Button3.Enabled = False
            Button4.Enabled = False
            Button5.Enabled = False
            Button6.Enabled = False
            Button8.Enabled = False
        Else
            Button7.Enabled = True
            Button3.Enabled = True
            Button4.Enabled = True
            Button5.Enabled = True
            Button6.Enabled = True
            Button8.Enabled = True
        End If
    End Sub

    '''''''''''''''''''''MAIN CONNECT'''''''''''''''''
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Dim thdconnect As New Thread(New ThreadStart(AddressOf athread))
        thdconnect.Start()

    End Sub

    Public Sub athread()
        Dim Address As String
        'Dim PortNumber As Integer
        If Button2.Text = "Connect" Then
            sClient = New Socket(AddressFamily.InterNetwork, _
                              SocketType.Stream, _
                              ProtocolType.Tcp)
            If TextBox3.Text = Nothing Or TextBox2.Text = Nothing Then
                MessageBox.Show("Enter the IP Address and Port Number", "User input required", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                Address = TextBox2.Text
                'PortNumber = Integer.Parse(TextBox3.Text)
                'Dim ipHost As IPHostEntry = Dns.GetHostEntry(Address)
                
                'Dim ipAddr As IPAddress = System.Net.Dns.GetHostEntry(Address).AddressList(0)
                'Dim Endpoint As New IPEndPoint(ipAddr, CInt(TextBox3.Text))
                Try

                    Dim ipAddr As IPAddress = System.Net.Dns.GetHostEntry(Address).AddressList(0)
                    Dim Endpoint As New IPEndPoint(ipAddr, CInt(TextBox3.Text))
                    sClient.BeginConnect(Endpoint, AddressOf ConnectCallback, Nothing)
                    'wait till it connects
                    ConnectDone.WaitOne()
                    ConnectDone.Reset()
                    Dim cb As New SimpleCallback(AddressOf ConnectedUI)
                    Me.Invoke(cb)
                    reader = "N"
                    SendData(Nothing, Nothing)
                    SendDone.WaitOne()
                    SendDone.Reset()
                    'MessageBox.Show(Address & PortNumber)
                Catch ex As Exception
                    MessageBox.Show("Failed to connect to the server:" & vbCrLf & _
                                    "Error was probably caused due to a timeout" & vbCrLf & _
                                    "Check if the network is available" & vbCrLf & _
                                    "Check the IP address and port number" & vbCrLf & _
                                    "", "Error in connection", _
                                    MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return
                    'MessageBox.Show(ex.ToString)
                End Try
            End If
        Else
            Try
                reader = "Q"
                SendData(Nothing, Nothing)
                SendDone.WaitOne()
                SendDone.Reset()
                SyncLock sClient
                    sClient.Shutdown(SocketShutdown.Both)
                    sClient.Close()
                End SyncLock
            Catch ex As Exception
                MessageBox.Show("Could not confirm if the connection was disconnected")
                'MessageBox.Show(ex.ToString)
            End Try
            CallDisconnectedUI()
        End If
        ConnectDone.Reset()

    End Sub

    Public Sub ConnectCallback(ByVal ar As IAsyncResult)
        Try
            'sClient = CType(ar.AsyncState, Socket)

            'BeginInvoke(New connectedDelegate(AddressOf ConnectedUI), sClient.RemoteEndPoint.ToString())
            SyncLock sClient
                sClient.EndConnect(ar)
                ConnectDone.Set()
            End SyncLock

        Catch ex As Exception
            MessageBox.Show("Failed to connect to the server: " & vbCrLf & _
                            "Error was probably caused due to a timeout" & vbCrLf & _
                            "Check if the network is available" & vbCrLf & _
                            "Check the IP address and port number" & vbCrLf _
                            , "Error in connection", _
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        Try
            sClient.BeginReceive(recvBuffer, 0, recvBuffer.Length, _
    SocketFlags.None, AddressOf ReceivedData, Nothing)
        Catch ex As Exception
        End Try
    End Sub

    'Public Sub connected(ByVal RemoteEndpoint As String)
    '    'ListBox4.Items.Add("Socket connected to: " & RemoteEndpoint)
    '    'ListBox4.SelectedIndex = ListBox4.Items.Count - 1
    '    TextBox4.AppendText("Socket connected to: " & vbCrLf) ' & RemoteEndpoint & vbCrLf)
    '    TextBox4.SelectionStart = TextBox4.Text.Length
    'End Sub
    Public Sub ConnectedUI()
        Dim RemoteServer As String
        RemoteServer = sClient.RemoteEndPoint.ToString
        TextBox4.AppendText("Established a connection with: " & RemoteServer & vbCrLf)
        TextBox4.SelectionStart = TextBox4.Text.Length
        Button2.Text = "Disconnect"
        RealTimeButton.Enabled = True
        If PGMButton.Checked = True Then
            Button7.Enabled = True
        End If
        Me.AcceptButton = Nothing
    End Sub

    Private Sub CallDisconnectedUI()
        Dim cb As New SimpleCallback(AddressOf DisconnectedUI)
        Me.Invoke(cb)
    End Sub

    Public Sub DisconnectedUI()
        'ListBox4.Items.Add("connection closed")
        'ListBox4.SelectedIndex = ListBox4.Items.Count - 1
        TextBox4.AppendText("Connection Terminated" & vbCrLf)
        TextBox4.SelectionStart = TextBox4.Text.Length
        Button2.Text = "Connect"
        RealTimeButton.Enabled = False
        PGMButton.Checked = True
        Button7.Enabled = False
        Me.AcceptButton = Nothing
    End Sub

    Public Sub ReceivedData(ByVal ar As IAsyncResult)
        Dim numBytes As Int32

        Try
            SyncLock sClient
                numBytes = sClient.EndReceive(ar)
            End SyncLock
        Catch ex As Exception
            Return
        End Try

        If numBytes = 0 Then
            Return
        End If

        '-- We have data!
        Dim data As String = _
            System.Text.ASCIIEncoding.ASCII.GetString(recvBuffer, 0, numBytes)
        CallDisplayTextCallback(data)

        '-- Start Receiving Data Again!
        sClient.BeginReceive(recvBuffer, 0, recvBuffer.Length, _
            SocketFlags.None, AddressOf ReceivedData, Nothing)
    End Sub

    Private Sub CallDisplayTextCallback(ByVal Text As String)
        Dim cb As New DisplayTextCallback(AddressOf DisplayText)
        Dim args() As Object = {Text}
        Me.Invoke(cb, args)
    End Sub


    Public Sub DisplayText(ByVal Text As String)
        TextBox4.AppendText("Data received from server: " & Text & vbCrLf)
        TextBox4.SelectionStart = TextBox4.Text.Length
    End Sub
End Class
