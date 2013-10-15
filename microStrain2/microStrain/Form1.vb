Imports System
Imports System.IO.Ports
Imports System.Text.Encoding
Imports System.Threading

Public Class Form1
    Dim idval As Integer
    Public Shared SendDone As ManualResetEvent = New ManualResetEvent(False)
    Shared _serialPort As SerialPort
    Dim SaveFilename As String
    Dim saveAllowed As Boolean = False
    Dim timeThread As New Thread(New ThreadStart(AddressOf pollSensor))
    Private Delegate Sub SimpleCallback(ByVal data As Object)
    Dim timeCB As TimerCallback = AddressOf timedSub
    Dim t As New Timer(timeCB)
    Dim cb1 As New SimpleCallback(AddressOf displayTime)
    Dim cb2 As New SimpleCallback(AddressOf displayOnGUI)
    Public Const AngleScale As Double = 360 / 65536

    Private portLock As New Object

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim ports As String() = SerialPort.GetPortNames()
        Dim port As String
        Label5.Text = Thread.CurrentThread.GetHashCode
        For Each port In ports
            ComboBox1.Items.Add(port)
        Next port
        _serialPort = New SerialPort()
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox1.TextChanged
        Try
            initSerialPort()
            t.Change(0, 100)
            'timeThread.IsBackground = True
            'timeThread.Priority = ThreadPriority.Highest
            'If timeThread.IsAlive = False Then timeThread.Start()

        Catch ex As Exception
            MessageBox.Show("1: " & ex.Message.ToString)
            If timeThread.IsAlive = True Then MessageBox.Show("10: " & "Thread alive")
            If (_serialPort.IsOpen = True) Then MessageBox.Show("11: " & "Serial " & _
                _serialPort.PortName.ToString & " is open")

        End Try
    End Sub

    Public Sub pollSensor()
        Dim dt As DateTime
        Dim sec_val As Integer
        ' Dim prevSec As Integer
        Try
            While True
                dt = DateTime.Now
                If ((dt.Millisecond Mod 100) = 0) And (dt.Millisecond <> sec_val) Then
                    saveData(" ")
                    sec_val = dt.Millisecond
                    'testSerial()
                    Me.Invoke(cb1, sec_val)
                    'saveData(sec_val.ToString)
                    saveData(" ")
                End If
            End While
        Catch ex As Exception
            MessageBox.Show("9: " & ex.ToString)
        End Try

    End Sub

    Public Sub displayTime(ByVal sec_val As Integer)
        TextBox2.Text = sec_val.ToString
    End Sub

    Public Sub testSerial()

        'Dim tx_buf() As Byte = {&HE}   'Command for Stab Euler 3DM-GX1
        Dim tx_buf() As Byte = {&H30, &HF1, &H32, &H33, &H34, &H35, &H36, &H37, &H38, &H39, &H3A}
        Dim rx_buf(512) As Byte
        Dim rx_len As Integer

        Try
            If _serialPort.IsOpen = True Then _serialPort.Write(tx_buf, 0, 11)
            Thread.Sleep(20)
            If _serialPort.IsOpen = True Then
                rx_len = _serialPort.Read(rx_buf, 0, rx_buf.Length)
                getEuler(rx_buf)
                Me.Invoke(cb2, rx_buf)
                If rx_len = 1 Then
                    ToolStripStatusLabel1.Text = "Received " & rx_len.ToString & " byte"
                Else
                    ToolStripStatusLabel1.Text = "Received " & rx_len.ToString & " bytes"
                End If
            End If
        Catch ex As Exception
            If TypeOf ex Is TimeoutException Then
                ToolStripStatusLabel1.Text = "TIMEOUT. Please check connections"
                Try
                    rx_buf(0) = &H3F
                    Me.Invoke(cb2, rx_buf)
                Catch ex2 As Exception
                    MessageBox.Show("2: " & ex2.ToString)
                End Try
            ElseIf TypeOf ex Is InvalidOperationException Then
                MessageBox.Show("5: " & ex.ToString)
            ElseIf TypeOf ex Is IO.IOException Then
                MessageBox.Show("7: " & ex.ToString)
            Else
                MessageBox.Show("8: " & ex.ToString)
            End If
        Finally
            Try
                If _serialPort.IsOpen = True Then
                    _serialPort.DiscardInBuffer()
                    _serialPort.DiscardOutBuffer()
                End If
            Catch ex As Exception
                MessageBox.Show("6: " & ex.ToString)
                If (_serialPort.IsOpen = True) Then MessageBox.Show("12: " & "Serial " & _
                    _serialPort.PortName.ToString & " is open")
            End Try
        End Try
    End Sub

    Sub displayOnGUI(ByVal rx_buf() As Byte)
        Dim newByte As Byte = rx_buf(0)
        ' TextBox1.Text = Unicode.GetChars(rx_buf)
        TextBox1.Text = ASCII.GetChars(rx_buf)
    End Sub

    Sub timedSub(ByVal state As Object)
        idval = idval + 1
        saveData(idval.ToString & " :0:  " & Thread.CurrentThread.GetHashCode & " ::: ")
        testSerial()
        saveData(idval.ToString & " :1:  " & Thread.CurrentThread.GetHashCode & " ::: ")

    End Sub

    Sub initSerialPort()
        Try
            ComboBox1.Text = ComboBox1.SelectedItem.ToString
            closeSerialPort()
            _serialPort.PortName = ComboBox1.Text
            _serialPort.BaudRate = 38400
            _serialPort.Parity = Parity.None
            _serialPort.StopBits = StopBits.One
            _serialPort.DataBits = 8
            _serialPort.DtrEnable = False
            _serialPort.Handshake = Handshake.None
            _serialPort.RtsEnable = False
            _serialPort.ReadTimeout = 1000
            _serialPort.WriteTimeout = 1000
            If _serialPort.IsOpen = False Then _serialPort.Open()
        Catch ex As Exception
            MessageBox.Show("3: " & ex.ToString)
        End Try
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If Button1.Text = "Capture Data" Then
            Dim saveFileDialog1 As New SaveFileDialog()
            saveFileDialog1.Filter = "Text File|*.txt"
            saveFileDialog1.Title = "Save Data"
            saveFileDialog1.ShowDialog()
            SaveFilename = saveFileDialog1.FileName.ToString
            saveAllowed = True
            Button1.Text = "Stop Data Capture"
        Else
            Button1.Text = "Capture Data"
            saveAllowed = False
        End If
    End Sub

    Public Sub saveData(ByVal Data As String)
        'dim data as string = "hello world"
        Try
            If saveAllowed = True Then
                'My.Computer.FileSystem.WriteAllText(SaveFilename, Data & vbCrLf, True, System.Text.Encoding.ASCII)
                My.Computer.FileSystem.WriteAllText(SaveFilename, Data & DateTime.Now.ToString("HH:mm:ss.fff") & vbCrLf, _
                                                    True, System.Text.Encoding.ASCII)

            End If
        Catch ex As Exception
            MessageBox.Show("10: " & ex.ToString)
        End Try
    End Sub

    Sub closeSerialPort()

        Try
            If (_serialPort.IsOpen = True) Then
                't.Dispose()
                't.Change(Timeout.Infinite, Timeout.Infinite)
                'timeThread.Abort()
                'timeThread.Join(5000)
                _serialPort.Close()
            End If
        Catch ex As Exception
            MessageBox.Show("4: " & ex.ToString)
        Finally

        End Try
    End Sub

    Public Sub getEuler(ByVal rawBytes() As Byte)
        Dim rawRoll, rawPitch, rawYaw, timerTicks As Short
        Dim roll, pitch, yaw As Double
        rawRoll = mergeMSBLSB(rawBytes(1), rawBytes(2))
        rawPitch = mergeMSBLSB(rawBytes(3), rawBytes(4))
        rawYaw = mergeMSBLSB(rawBytes(5), rawBytes(6))
        timerTicks = mergeMSBLSB(rawBytes(7), rawBytes(8))
        'roll = CType(rawRoll, Double) * AngleScale
        'pitch = CType(rawPitch, Double) * AngleScale
        'yaw = CType(rawYaw, Double) * AngleScale
        'MessageBox.Show("RawRoll: " & roll.ToString & " " & rawBytes(0))
        'MessageBox.Show("RawRoll: " & rawRoll & " " & rawBytes(0))
    End Sub

    Public Function mergeMSBLSB(ByVal MSB As Byte, ByVal LSB As Byte) As Short
        Dim temp As Short
        temp = CType(MSB, Short)
        temp = temp << 8
        temp = temp Or CType(LSB, Short)
        Return temp
    End Function

    Private Sub Form1_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        'timeThread.Abort()
    End Sub

    Private Sub Form1_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        'timeThread.Abort()
        closeSerialPort()
    End Sub
End Class
