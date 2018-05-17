Imports Microsoft.VisualBasic
Imports System
Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Data
Imports Atalasoft.Imaging.WinControls
Imports Atalasoft.Barcoding.Reading
Imports Atalasoft.Imaging.Codec
Imports Atalasoft.Imaging.ImageProcessing.Document
Imports Atalasoft.Imaging

Namespace BarcodeDemo
    ''' <summary>
    ''' Demonstration of Atalasoft barcode recognition.
    ''' </summary>
    ''' 
    Public Class Barcoder
        Inherits System.Windows.Forms.Form

        Dim _tmpImg As AtalaImage = Nothing

        ' To follow the process of recognition of a barcode, recognizeButton_Click
        ' and recognizeBarcodes are the main methods that should be examined.
        ' recognizeButton_Click handles nearly all the user-interface feedback
        ' leaving the work of recognition to recognizeBarcodes, which is written
        ' as much as possible to be separate from the UI and the specific
        ' implementation.
        '
        ' Various options are set in the callback methods.

        Private Sub recognizeButton_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles recognizeButton.Click
            ' Respond to a recognize request - this button is disabled if it's not
            ' appropriate to do a recognize (options that don't make sense,
            ' no file loaded)
            Dim results As BarCode() = Nothing


            ' swap in a wait cursor
            Dim savedCursor As System.Windows.Forms.Cursor = Me.Cursor
            Me.Cursor = System.Windows.Forms.Cursors.WaitCursor

            Dim start, [end] As DateTime
            Dim elapsed As TimeSpan

            ' time the process
            start = DateTime.Now

            If workspaceViewer.Image Is Nothing Then
                MessageBox.Show("An image must be loaded first")
                Return
            End If
            Using readEngine As New BarCodeReader(workspaceViewer.Image, Me.useAutomaticThresholding_CheckBox.Checked)
                results = recognizeBarcodes(readEngine, options)
            End Using

            [end] = DateTime.Now
            elapsed = [end].Subtract(start)

            Me.Cursor = savedCursor

            If results.Length = 1 Then
                statusBox.AppendText(results.Length & " total barcode" & ("") & " found." & Constants.vbCrLf)
            Else
                statusBox.AppendText(results.Length & " total barcode" & ("s") & " found." & Constants.vbCrLf)
            End If

            If Not results Is Nothing AndAlso results.Length > 0 Then
                If results.Length > 1 Then
                    statusBox.AppendText("Found " & results.Length & " barcode" & ("s") & ":" & Constants.vbCrLf)
                Else
                    statusBox.AppendText("Found " & results.Length & " barcode" & ("") & ":" & Constants.vbCrLf)
                End If
                For i As Integer = 0 To results.Length - 1
                    statusBox.AppendText("      Result #" & (i + 1) & Constants.vbCrLf)
                    statusBox.AppendText("           Direction: " & results(i).ReadDirection.ToString() & Constants.vbCrLf)
                    statusBox.AppendText("           Symbology: " & symbologyToString(results(i).Symbology, mySymbologyMap) & Constants.vbCrLf)
                    statusBox.AppendText("           Text read: " & results(i).DataString & Constants.vbCrLf)
                Next i
            End If
            statusBox.AppendText("Total time: " & System.String.Format("{0:0.000}", elapsed.TotalSeconds) & " seconds." & Constants.vbCrLf)

            finalResults = results
            workspaceViewer.Invalidate()
        End Sub

        ' Read a set of barcodes from an image.
        ' 
        Private Function recognizeBarcodes(ByVal reader As BarCodeReader, ByVal optionsIn As ReadOpts) As BarCode()

            Dim results As BarCode() = Nothing
            Dim options As ReadOpts = New ReadOpts(optionsIn)

            If options.Symbology = 0 Then
                Return Nothing
            End If

            Try
                results = reader.ReadBars(options)
            Catch ex As ArgumentOutOfRangeException
                statusBox.AppendText("Range error in options: " & ex.Message & Constants.vbCrLf)
            Catch ex As System.Exception
                statusBox.AppendText("General error: " & ex.Message & Constants.vbCrLf)
            End Try

            Return results
        End Function

        ' private class for mapping internal symbology names into
        ' the user interface
        Private Class SymbologyMap
            Public Sub New(ByVal name As String, ByVal sym As Symbologies)
                UIName = name
                symbology = sym
            End Sub
            ' ToString is vital - it allows this object to live transparently
            ' in a ListBox and have its UIName displayed.
            Public Overrides Function ToString() As String
                Return UIName
            End Function
            Public UIName As String
            Public symbology As Symbologies
        End Class

        ' private class for mapping internal scan directions into
        ' the user interface
        Private Class ScanDirectionMap
            Public Sub New(ByVal name As String, ByVal dir As Directions)
                UIName = name
                direction = dir
            End Sub
            ' ToString is vital - it allows this object to live transparently
            ' in a ListBox and have its UIName displayed.
            Public Overrides Function ToString() As String
                Return UIName
            End Function
            Public UIName As String
            Public direction As Directions
        End Class

        ' members used for barcode recognition
        ' maps from internal enumerations to UIStrings
        Private mySymbologyMap As SymbologyMap()
        Private directionMap As ScanDirectionMap()

        Private finalResults As BarCode() = Nothing
        Private options As ReadOpts
        Private imageLoaded As Boolean = False

        ' members used for the UI
        Private WithEvents workspaceViewer As Atalasoft.Imaging.WinControls.WorkspaceViewer
        Private label1 As System.Windows.Forms.Label
        Private WithEvents statusBox As System.Windows.Forms.TextBox
        Private WithEvents barcodeSymbologyList As System.Windows.Forms.CheckedListBox
        Private WithEvents openButton As System.Windows.Forms.Button
        Private WithEvents recognizeButton As System.Windows.Forms.Button
        Private groupBox1 As System.Windows.Forms.GroupBox
        Private groupBox2 As System.Windows.Forms.GroupBox
        Private WithEvents scanDirectionList As System.Windows.Forms.CheckedListBox
        Private groupBox3 As System.Windows.Forms.GroupBox
        Private groupBox4 As System.Windows.Forms.GroupBox
        Private WithEvents expectedBarCodes As System.Windows.Forms.TrackBar
        Private expectedBarcodesLabel As System.Windows.Forms.Label
        Private WithEvents scanInterval As System.Windows.Forms.TrackBar
        Private scanIntervalLabel As System.Windows.Forms.Label
        Private WithEvents shrinkToFit As System.Windows.Forms.CheckBox
        Private WithEvents showBoundingRects As System.Windows.Forms.CheckBox
        Private WithEvents showBoundingBoxes As System.Windows.Forms.CheckBox
        Private WithEvents aboutBtn As System.Windows.Forms.Button
        Private openFileDialog1 As System.Windows.Forms.OpenFileDialog
        Friend WithEvents btnClearSymbologies As System.Windows.Forms.Button
        Friend WithEvents btnSelectAllSymbologies As System.Windows.Forms.Button
        Friend WithEvents btnClearAllDirections As System.Windows.Forms.Button
        Friend WithEvents btnSelectAllDirections As System.Windows.Forms.Button
        Friend WithEvents useAutomaticThresholding_CheckBox As System.Windows.Forms.CheckBox
        Friend WithEvents GroupBox5 As System.Windows.Forms.GroupBox
        Friend WithEvents btnMorphoErode As System.Windows.Forms.RadioButton
        Friend WithEvents btnMorphoDilate As System.Windows.Forms.RadioButton
        Friend WithEvents btnMorphoNone As System.Windows.Forms.RadioButton

        ''' <summary>
        ''' Required designer variable.
        ''' </summary>
        Private components As System.ComponentModel.Container = Nothing

        Public Sub New()
            ' build the maps first since they get used by the UI
            ' so make sure that they're constructed before the UI
            ' gets built.
            mySymbologyMap = New SymbologyMap() {New SymbologyMap("Aztec", Symbologies.Aztec), _
                                                 New SymbologyMap("Australia Post", Symbologies.AustraliaPost), _
                                                 New SymbologyMap("Codabar", Symbologies.Codabar), _
                                                 New SymbologyMap("Code 11", Symbologies.Code11), _
                                                 New SymbologyMap("Code 128", Symbologies.Code128), _
                                                 New SymbologyMap("Code 32", Symbologies.Code32), _
                                                 New SymbologyMap("Code 39", Symbologies.Code39), _
                                                 New SymbologyMap("Code 93", Symbologies.Code93), _
                                                 New SymbologyMap("Data Matrix", Symbologies.Datamatrix), _
                                                 New SymbologyMap("Ean 13", Symbologies.Ean13), _
                                                 New SymbologyMap("Ean 8", Symbologies.Ean8), _
                                                 New SymbologyMap("I 2 of 5", Symbologies.I2of5), _
                                                 New SymbologyMap("Intelligent Mail", Symbologies.IntelligentMail), _
                                                 New SymbologyMap("ITF-14", Symbologies.Itf14), _
                                                 New SymbologyMap("Micro QR Code", Symbologies.MicroQr), _
                                                 New SymbologyMap("Patch", Symbologies.Patch), _
                                                 New SymbologyMap("PDF 417", Symbologies.Pdf417), _
                                                 New SymbologyMap("Planet", Symbologies.Planet), _
                                                 New SymbologyMap("Plus 2", Symbologies.Plus2), _
                                                 New SymbologyMap("Plus 5", Symbologies.Plus5), _
                                                 New SymbologyMap("Postnet", Symbologies.Postnet), _
                                                 New SymbologyMap("QR", Symbologies.Qr), _
                                                 New SymbologyMap("Royal Mail +4 State Customer Code", Symbologies.Rm4scc), _
                                                 New SymbologyMap("RSS-14", Symbologies.Rss14), _
                                                 New SymbologyMap("RSS Limited", Symbologies.RssLimited), _
                                                 New SymbologyMap("Telepen", Symbologies.Telepen), _
                                                 New SymbologyMap("UPC A", Symbologies.Upca), _
                                                 New SymbologyMap("UPC E", Symbologies.Upce)}

            directionMap = New ScanDirectionMap() {New ScanDirectionMap("Left to Right", Directions.East), _
                                                   New ScanDirectionMap("Right to Left", Directions.West), _
                                                   New ScanDirectionMap("Top to Bottom", Directions.South), _
                                                   New ScanDirectionMap("Bottom to Top", Directions.North), _
                                                   New ScanDirectionMap("Bottom Left to Top Right", Directions.NorthEast), _
                                                   New ScanDirectionMap("Top Left to Bottom Right", Directions.SouthEast), _
                                                   New ScanDirectionMap("Top Right to Bottom Left", Directions.SouthWest), _
                                                   New ScanDirectionMap("Bottom Right to Top Left", Directions.NorthWest)}

            ' Ensure we'll open the image with one of our licensed decoders
            AtalaDemos.HelperMethods.PopulateDecoders(RegisteredDecoders.Decoders)

            ' Sets the default Pixel Format Changer to use Thresholding.
            ' This may cause problems for people with PhotoFree or PhotoPro licenses.
            AtalaImage.PixelFormatChanger = New DocumentPixelFormatChanger(New AtalaPixelFormatChanger())

            InitializeComponent()
        End Sub

        ''' <summary>
        ''' Clean up any resources being used.
        ''' </summary>
        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                If Not components Is Nothing Then
                    components.Dispose()
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

#Region "Windows Form Designer generated code"
        ''' <summary>
        ''' Required method for Designer support - do not modify
        ''' the contents of this method with the code editor.
        ''' </summary>
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Barcoder))
            Me.workspaceViewer = New Atalasoft.Imaging.WinControls.WorkspaceViewer
            Me.label1 = New System.Windows.Forms.Label
            Me.openButton = New System.Windows.Forms.Button
            Me.recognizeButton = New System.Windows.Forms.Button
            Me.statusBox = New System.Windows.Forms.TextBox
            Me.barcodeSymbologyList = New System.Windows.Forms.CheckedListBox
            Me.groupBox1 = New System.Windows.Forms.GroupBox
            Me.btnClearSymbologies = New System.Windows.Forms.Button
            Me.btnSelectAllSymbologies = New System.Windows.Forms.Button
            Me.groupBox2 = New System.Windows.Forms.GroupBox
            Me.btnClearAllDirections = New System.Windows.Forms.Button
            Me.btnSelectAllDirections = New System.Windows.Forms.Button
            Me.scanDirectionList = New System.Windows.Forms.CheckedListBox
            Me.groupBox3 = New System.Windows.Forms.GroupBox
            Me.scanIntervalLabel = New System.Windows.Forms.Label
            Me.scanInterval = New System.Windows.Forms.TrackBar
            Me.groupBox4 = New System.Windows.Forms.GroupBox
            Me.expectedBarcodesLabel = New System.Windows.Forms.Label
            Me.expectedBarCodes = New System.Windows.Forms.TrackBar
            Me.shrinkToFit = New System.Windows.Forms.CheckBox
            Me.showBoundingRects = New System.Windows.Forms.CheckBox
            Me.showBoundingBoxes = New System.Windows.Forms.CheckBox
            Me.aboutBtn = New System.Windows.Forms.Button
            Me.openFileDialog1 = New System.Windows.Forms.OpenFileDialog
            Me.useAutomaticThresholding_CheckBox = New System.Windows.Forms.CheckBox
            Me.GroupBox5 = New System.Windows.Forms.GroupBox
            Me.btnMorphoNone = New System.Windows.Forms.RadioButton
            Me.btnMorphoDilate = New System.Windows.Forms.RadioButton
            Me.btnMorphoErode = New System.Windows.Forms.RadioButton
            Me.groupBox1.SuspendLayout()
            Me.groupBox2.SuspendLayout()
            Me.groupBox3.SuspendLayout()
            CType(Me.scanInterval, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.groupBox4.SuspendLayout()
            CType(Me.expectedBarCodes, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.GroupBox5.SuspendLayout()
            Me.SuspendLayout()
            '
            'workspaceViewer
            '
            Me.workspaceViewer.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                        Or System.Windows.Forms.AnchorStyles.Left) _
                        Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.workspaceViewer.AntialiasDisplay = Atalasoft.Imaging.WinControls.AntialiasDisplayMode.ScaleToGray
            Me.workspaceViewer.DisplayProfile = Nothing
            Me.workspaceViewer.Location = New System.Drawing.Point(30, 49)
            Me.workspaceViewer.Magnifier.BackColor = System.Drawing.Color.White
            Me.workspaceViewer.Magnifier.BorderColor = System.Drawing.Color.Black
            Me.workspaceViewer.Magnifier.Size = New System.Drawing.Size(100, 100)
            Me.workspaceViewer.Name = "workspaceViewer"
            Me.workspaceViewer.OutputProfile = Nothing
            Me.workspaceViewer.Selection = Nothing
            Me.workspaceViewer.Size = New System.Drawing.Size(444, 228)
            Me.workspaceViewer.TabIndex = 0
            Me.workspaceViewer.Text = "The Image"
            '
            'label1
            '
            Me.label1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                        Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.label1.Font = New System.Drawing.Font("Arial Black", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.label1.ForeColor = System.Drawing.Color.Orange
            Me.label1.Location = New System.Drawing.Point(12, -3)
            Me.label1.Name = "label1"
            Me.label1.Size = New System.Drawing.Size(476, 27)
            Me.label1.TabIndex = 1
            Me.label1.Text = "Atalasoft Barcode Reader Demo"
            Me.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            Me.label1.UseMnemonic = False
            '
            'openButton
            '
            Me.openButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
            Me.openButton.Location = New System.Drawing.Point(20, 283)
            Me.openButton.Name = "openButton"
            Me.openButton.Size = New System.Drawing.Size(96, 23)
            Me.openButton.TabIndex = 2
            Me.openButton.Text = "Open Image..."
            '
            'recognizeButton
            '
            Me.recognizeButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
            Me.recognizeButton.Location = New System.Drawing.Point(132, 283)
            Me.recognizeButton.Name = "recognizeButton"
            Me.recognizeButton.Size = New System.Drawing.Size(80, 24)
            Me.recognizeButton.TabIndex = 4
            Me.recognizeButton.Text = "Recognize..."
            '
            'statusBox
            '
            Me.statusBox.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                        Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.statusBox.Location = New System.Drawing.Point(12, 467)
            Me.statusBox.Multiline = True
            Me.statusBox.Name = "statusBox"
            Me.statusBox.ReadOnly = True
            Me.statusBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
            Me.statusBox.Size = New System.Drawing.Size(468, 72)
            Me.statusBox.TabIndex = 5
            '
            'barcodeSymbologyList
            '
            Me.barcodeSymbologyList.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                        Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.barcodeSymbologyList.CheckOnClick = True
            Me.barcodeSymbologyList.Location = New System.Drawing.Point(8, 16)
            Me.barcodeSymbologyList.Name = "barcodeSymbologyList"
            Me.barcodeSymbologyList.Size = New System.Drawing.Size(174, 64)
            Me.barcodeSymbologyList.TabIndex = 6
            '
            'groupBox1
            '
            Me.groupBox1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                        Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.groupBox1.Controls.Add(Me.btnClearSymbologies)
            Me.groupBox1.Controls.Add(Me.btnSelectAllSymbologies)
            Me.groupBox1.Controls.Add(Me.barcodeSymbologyList)
            Me.groupBox1.Location = New System.Drawing.Point(220, 283)
            Me.groupBox1.Name = "groupBox1"
            Me.groupBox1.Size = New System.Drawing.Size(260, 88)
            Me.groupBox1.TabIndex = 7
            Me.groupBox1.TabStop = False
            Me.groupBox1.Text = "Barcode Types"
            '
            'btnClearSymbologies
            '
            Me.btnClearSymbologies.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.btnClearSymbologies.Location = New System.Drawing.Point(188, 57)
            Me.btnClearSymbologies.Name = "btnClearSymbologies"
            Me.btnClearSymbologies.Size = New System.Drawing.Size(64, 23)
            Me.btnClearSymbologies.TabIndex = 8
            Me.btnClearSymbologies.Text = "Clear All"
            Me.btnClearSymbologies.UseVisualStyleBackColor = True
            '
            'btnSelectAllSymbologies
            '
            Me.btnSelectAllSymbologies.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.btnSelectAllSymbologies.Location = New System.Drawing.Point(188, 16)
            Me.btnSelectAllSymbologies.Name = "btnSelectAllSymbologies"
            Me.btnSelectAllSymbologies.Size = New System.Drawing.Size(64, 32)
            Me.btnSelectAllSymbologies.TabIndex = 7
            Me.btnSelectAllSymbologies.Text = "Select All"
            Me.btnSelectAllSymbologies.UseVisualStyleBackColor = True
            '
            'groupBox2
            '
            Me.groupBox2.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                        Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.groupBox2.Controls.Add(Me.btnClearAllDirections)
            Me.groupBox2.Controls.Add(Me.btnSelectAllDirections)
            Me.groupBox2.Controls.Add(Me.scanDirectionList)
            Me.groupBox2.Location = New System.Drawing.Point(220, 371)
            Me.groupBox2.Name = "groupBox2"
            Me.groupBox2.Size = New System.Drawing.Size(260, 88)
            Me.groupBox2.TabIndex = 8
            Me.groupBox2.TabStop = False
            Me.groupBox2.Text = "Scan Directions"
            '
            'btnClearAllDirections
            '
            Me.btnClearAllDirections.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.btnClearAllDirections.Location = New System.Drawing.Point(188, 57)
            Me.btnClearAllDirections.Name = "btnClearAllDirections"
            Me.btnClearAllDirections.Size = New System.Drawing.Size(64, 23)
            Me.btnClearAllDirections.TabIndex = 9
            Me.btnClearAllDirections.Text = "Clear All"
            Me.btnClearAllDirections.UseVisualStyleBackColor = True
            '
            'btnSelectAllDirections
            '
            Me.btnSelectAllDirections.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.btnSelectAllDirections.Location = New System.Drawing.Point(188, 16)
            Me.btnSelectAllDirections.Name = "btnSelectAllDirections"
            Me.btnSelectAllDirections.Size = New System.Drawing.Size(64, 32)
            Me.btnSelectAllDirections.TabIndex = 9
            Me.btnSelectAllDirections.Text = "Select All"
            Me.btnSelectAllDirections.UseVisualStyleBackColor = True
            '
            'scanDirectionList
            '
            Me.scanDirectionList.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                        Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.scanDirectionList.CheckOnClick = True
            Me.scanDirectionList.Location = New System.Drawing.Point(8, 16)
            Me.scanDirectionList.Name = "scanDirectionList"
            Me.scanDirectionList.Size = New System.Drawing.Size(174, 64)
            Me.scanDirectionList.TabIndex = 9
            '
            'groupBox3
            '
            Me.groupBox3.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
            Me.groupBox3.Controls.Add(Me.scanIntervalLabel)
            Me.groupBox3.Controls.Add(Me.scanInterval)
            Me.groupBox3.Location = New System.Drawing.Point(20, 315)
            Me.groupBox3.Name = "groupBox3"
            Me.groupBox3.Size = New System.Drawing.Size(192, 64)
            Me.groupBox3.TabIndex = 9
            Me.groupBox3.TabStop = False
            Me.groupBox3.Text = "Scan Interval"
            '
            'scanIntervalLabel
            '
            Me.scanIntervalLabel.Location = New System.Drawing.Point(88, 16)
            Me.scanIntervalLabel.Name = "scanIntervalLabel"
            Me.scanIntervalLabel.Size = New System.Drawing.Size(24, 16)
            Me.scanIntervalLabel.TabIndex = 1
            Me.scanIntervalLabel.Text = "5"
            Me.scanIntervalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            Me.scanIntervalLabel.UseMnemonic = False
            '
            'scanInterval
            '
            Me.scanInterval.AutoSize = False
            Me.scanInterval.Location = New System.Drawing.Point(8, 40)
            Me.scanInterval.Maximum = 20
            Me.scanInterval.Minimum = 1
            Me.scanInterval.Name = "scanInterval"
            Me.scanInterval.Size = New System.Drawing.Size(176, 20)
            Me.scanInterval.TabIndex = 0
            Me.scanInterval.TickFrequency = 5
            Me.scanInterval.Value = 5
            '
            'groupBox4
            '
            Me.groupBox4.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
            Me.groupBox4.Controls.Add(Me.expectedBarcodesLabel)
            Me.groupBox4.Controls.Add(Me.expectedBarCodes)
            Me.groupBox4.Location = New System.Drawing.Point(20, 395)
            Me.groupBox4.Name = "groupBox4"
            Me.groupBox4.Size = New System.Drawing.Size(192, 64)
            Me.groupBox4.TabIndex = 10
            Me.groupBox4.TabStop = False
            Me.groupBox4.Text = "Number of Expected Barcodes"
            '
            'expectedBarcodesLabel
            '
            Me.expectedBarcodesLabel.Location = New System.Drawing.Point(88, 16)
            Me.expectedBarcodesLabel.Name = "expectedBarcodesLabel"
            Me.expectedBarcodesLabel.Size = New System.Drawing.Size(24, 16)
            Me.expectedBarcodesLabel.TabIndex = 2
            Me.expectedBarcodesLabel.Text = "1"
            Me.expectedBarcodesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            Me.expectedBarcodesLabel.UseMnemonic = False
            '
            'expectedBarCodes
            '
            Me.expectedBarCodes.AutoSize = False
            Me.expectedBarCodes.Location = New System.Drawing.Point(8, 40)
            Me.expectedBarCodes.Minimum = 1
            Me.expectedBarCodes.Name = "expectedBarCodes"
            Me.expectedBarCodes.Size = New System.Drawing.Size(176, 20)
            Me.expectedBarCodes.TabIndex = 1
            Me.expectedBarCodes.TickFrequency = 3
            Me.expectedBarCodes.Value = 1
            '
            'shrinkToFit
            '
            Me.shrinkToFit.Location = New System.Drawing.Point(20, 25)
            Me.shrinkToFit.Name = "shrinkToFit"
            Me.shrinkToFit.Size = New System.Drawing.Size(120, 16)
            Me.shrinkToFit.TabIndex = 11
            Me.shrinkToFit.Text = "Shrink Image to Fit"
            '
            'showBoundingRects
            '
            Me.showBoundingRects.Checked = True
            Me.showBoundingRects.CheckState = System.Windows.Forms.CheckState.Checked
            Me.showBoundingRects.Location = New System.Drawing.Point(148, 25)
            Me.showBoundingRects.Name = "showBoundingRects"
            Me.showBoundingRects.Size = New System.Drawing.Size(168, 16)
            Me.showBoundingRects.TabIndex = 12
            Me.showBoundingRects.Text = "Show Bounding Rectangles"
            '
            'showBoundingBoxes
            '
            Me.showBoundingBoxes.Checked = True
            Me.showBoundingBoxes.CheckState = System.Windows.Forms.CheckState.Checked
            Me.showBoundingBoxes.Location = New System.Drawing.Point(324, 25)
            Me.showBoundingBoxes.Name = "showBoundingBoxes"
            Me.showBoundingBoxes.Size = New System.Drawing.Size(144, 16)
            Me.showBoundingBoxes.TabIndex = 13
            Me.showBoundingBoxes.Text = "Show Bounding Boxes"
            '
            'aboutBtn
            '
            Me.aboutBtn.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.aboutBtn.Location = New System.Drawing.Point(404, 602)
            Me.aboutBtn.Name = "aboutBtn"
            Me.aboutBtn.Size = New System.Drawing.Size(88, 24)
            Me.aboutBtn.TabIndex = 14
            Me.aboutBtn.Text = "About ..."
            '
            'useAutomaticThresholding_CheckBox
            '
            Me.useAutomaticThresholding_CheckBox.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                        Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.useAutomaticThresholding_CheckBox.AutoSize = True
            Me.useAutomaticThresholding_CheckBox.Checked = True
            Me.useAutomaticThresholding_CheckBox.CheckState = System.Windows.Forms.CheckState.Checked
            Me.useAutomaticThresholding_CheckBox.Location = New System.Drawing.Point(16, 602)
            Me.useAutomaticThresholding_CheckBox.Name = "useAutomaticThresholding_CheckBox"
            Me.useAutomaticThresholding_CheckBox.Size = New System.Drawing.Size(346, 17)
            Me.useAutomaticThresholding_CheckBox.TabIndex = 15
            Me.useAutomaticThresholding_CheckBox.Text = "Enable Automatic Thresholding (Try Toggling If Codes Not Reading)"
            Me.useAutomaticThresholding_CheckBox.UseVisualStyleBackColor = True
            '
            'GroupBox5
            '
            Me.GroupBox5.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
            Me.GroupBox5.Controls.Add(Me.btnMorphoErode)
            Me.GroupBox5.Controls.Add(Me.btnMorphoDilate)
            Me.GroupBox5.Controls.Add(Me.btnMorphoNone)
            Me.GroupBox5.Location = New System.Drawing.Point(12, 560)
            Me.GroupBox5.Name = "GroupBox5"
            Me.GroupBox5.Size = New System.Drawing.Size(352, 36)
            Me.GroupBox5.TabIndex = 16
            Me.GroupBox5.TabStop = False
            Me.GroupBox5.Text = "Morphology Command to Apply"
            '
            'btnMorphoNone
            '
            Me.btnMorphoNone.AutoSize = True
            Me.btnMorphoNone.Checked = True
            Me.btnMorphoNone.Location = New System.Drawing.Point(6, 13)
            Me.btnMorphoNone.Name = "btnMorphoNone"
            Me.btnMorphoNone.Size = New System.Drawing.Size(51, 17)
            Me.btnMorphoNone.TabIndex = 0
            Me.btnMorphoNone.TabStop = True
            Me.btnMorphoNone.Text = "None"
            Me.btnMorphoNone.UseVisualStyleBackColor = True
            '
            'btnMorphoDilate
            '
            Me.btnMorphoDilate.AutoSize = True
            Me.btnMorphoDilate.Location = New System.Drawing.Point(148, 13)
            Me.btnMorphoDilate.Name = "btnMorphoDilate"
            Me.btnMorphoDilate.Size = New System.Drawing.Size(52, 17)
            Me.btnMorphoDilate.TabIndex = 1
            Me.btnMorphoDilate.Text = "Dilate"
            Me.btnMorphoDilate.UseVisualStyleBackColor = True
            '
            'btnMorphoErode
            '
            Me.btnMorphoErode.AutoSize = True
            Me.btnMorphoErode.Location = New System.Drawing.Point(283, 13)
            Me.btnMorphoErode.Name = "btnMorphoErode"
            Me.btnMorphoErode.Size = New System.Drawing.Size(53, 17)
            Me.btnMorphoErode.TabIndex = 2
            Me.btnMorphoErode.Text = "Erode"
            Me.btnMorphoErode.UseVisualStyleBackColor = True
            '
            'Barcoder
            '
            Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
            Me.ClientSize = New System.Drawing.Size(508, 632)
            Me.Controls.Add(Me.GroupBox5)
            Me.Controls.Add(Me.useAutomaticThresholding_CheckBox)
            Me.Controls.Add(Me.aboutBtn)
            Me.Controls.Add(Me.showBoundingBoxes)
            Me.Controls.Add(Me.showBoundingRects)
            Me.Controls.Add(Me.statusBox)
            Me.Controls.Add(Me.workspaceViewer)
            Me.Controls.Add(Me.shrinkToFit)
            Me.Controls.Add(Me.groupBox4)
            Me.Controls.Add(Me.groupBox3)
            Me.Controls.Add(Me.groupBox2)
            Me.Controls.Add(Me.groupBox1)
            Me.Controls.Add(Me.recognizeButton)
            Me.Controls.Add(Me.openButton)
            Me.Controls.Add(Me.label1)
            Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
            Me.Name = "Barcoder"
            Me.Text = "Atalasoft Barcode Reader Demo"
            Me.groupBox1.ResumeLayout(False)
            Me.groupBox2.ResumeLayout(False)
            Me.groupBox3.ResumeLayout(False)
            CType(Me.scanInterval, System.ComponentModel.ISupportInitialize).EndInit()
            Me.groupBox4.ResumeLayout(False)
            CType(Me.expectedBarCodes, System.ComponentModel.ISupportInitialize).EndInit()
            Me.GroupBox5.ResumeLayout(False)
            Me.GroupBox5.PerformLayout()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
#End Region

        ''' <summary>
        ''' The main entry point for the application.
        ''' </summary>
        <STAThread()> _
        Shared Sub Main()
            Application.Run(New Barcoder())
        End Sub

        Private Sub Barcoder_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
            recognizeButton.Enabled = False

            ' set a few reasonable default options
            options = New ReadOpts()
            options.Symbology = Symbologies.Code39
            options.Direction = Directions.East
            ' counter-intuitive - these defaults get pulled from the UI instead
            ' of being pushed into the UI
            options.ScanInterval = scanInterval.Value
            options.ScanBarsToRead = expectedBarCodes.Value

            ' map the options into the UI
            mapBarcodeSymbologies(barcodeSymbologyList, options, mySymbologyMap)
            mapScanDirections(scanDirectionList, options, directionMap)
            statusBox.AppendText("Application loaded.  Click ""Open Image"" to load an image." & Constants.vbCrLf)

            ' This should point to the "DotImage 4.0\Images\Barcodes" folder.
            Me.openFileDialog1.FileName = System.IO.Path.GetFullPath("..\..\Images\Barcodes\Code39Scanned.gif")
            If (Not System.IO.File.Exists(Me.openFileDialog1.FileName)) Then
                Me.openFileDialog1.FileName = System.IO.Path.GetFullPath("..\..\..\..\..\Images\Barcodes\Code39Scanned.gif")
                If (Not System.IO.File.Exists(Me.openFileDialog1.FileName)) Then
                    Me.openFileDialog1.FileName = ""
                End If
            End If
        End Sub

        Private Sub mapScanDirections(ByVal listBox As System.Windows.Forms.CheckedListBox, ByVal theOptions As ReadOpts, ByVal map As ScanDirectionMap())
            ' put each scan direction into the check box,
            ' checking it as needed.
            For i As Integer = 0 To map.Length - 1
                listBox.Items.Add(map(i))
                If (theOptions.Direction And map(i).direction) <> 0 Then
                    listBox.SetItemChecked(i, True)
                End If
            Next i
        End Sub

        Private Sub btnSelectAllDirections_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSelectAllDirections.Click
            CheckUncheckAllListItems(scanDirectionList, True)
        End Sub

        Private Sub btnClearAllDirections_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClearAllDirections.Click
            CheckUncheckAllListItems(scanDirectionList, False)
        End Sub

        Private Sub mapBarcodeSymbologies(ByVal listBox As System.Windows.Forms.CheckedListBox, ByVal theOptions As ReadOpts, ByVal map As SymbologyMap())
            ' put each symbology into the check box,
            ' checking it as needed.
            'For i As Integer = 0 To map.Length - 1
            '    listBox.Items.Add(map(i))
            '    If (theOptions.Symbology And map(i).symbology) <> 0 Then
            '        listBox.SetItemChecked(i, True)
            '    End If
            'Next i
            Dim reader As New BarCodeReader
            For i As Integer = 0 To map.Length - 1
                If reader.IsSymbologySupported(map(i).symbology) Then
                    listBox.Items.Add(map(i))
                    If (theOptions.Symbology And map(i).symbology) <> 0 Then
                        ' if we skipped any symbologies, then checking i will check the wrong one
                        ' so we check the most recent item ... last added
                        listBox.SetItemChecked(listBox.Items.Count - 1, True)
                    End If
                End If
            Next i
        End Sub

        Private Sub btnSelectAllSymbologies_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSelectAllSymbologies.Click
            CheckUncheckAllListItems(barcodeSymbologyList, True)
        End Sub

        Private Sub btnClearSymbologies_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClearSymbologies.Click
            CheckUncheckAllListItems(barcodeSymbologyList, False)
        End Sub


        Private Sub CheckUncheckAllListItems(ByVal listBox As System.Windows.Forms.CheckedListBox, ByVal status As Boolean)
            For i As Integer = 0 To listBox.Items.Count - 1
                listBox.SetItemChecked(i, status)
            Next
        End Sub

        ' Look up the string value of a given symbology
        Private Function symbologyToString(ByVal sym As Symbologies, ByVal map As SymbologyMap()) As String
            For i As Integer = 0 To map.Length - 1
                If sym = map(i).symbology Then
                    Return map(i).UIName
                End If
            Next i
            Return "Unknown"
        End Function

        ' Respond to a file open
        Private Sub openButton_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles openButton.Click

            'Try to locate images folder
            Dim imagesFolder As String = Application.ExecutablePath
            Dim pos As Integer = imagesFolder.IndexOf("DotImage ")
            If (pos <> -1) Then
                imagesFolder = imagesFolder.Substring(0, imagesFolder.IndexOf(System.IO.Path.DirectorySeparatorChar, pos)) + "\Images\Barcodes"
            End If

            'use this folder as a starting point
            Me.openFileDialog1.InitialDirectory = imagesFolder

            'Filter on the available, licensed decoders
            Me.openFileDialog1.Filter = AtalaDemos.HelperMethods.CreateDialogFilter(True)

            If Me.openFileDialog1.ShowDialog(Me) <> System.Windows.Forms.DialogResult.OK Then
                Return
            End If

            Try
                Me._tmpImg = New AtalaImage(openFileDialog1.FileName)

                ' Loads the image into the viewer - applying the desired morphology if needed
                UpdateMorphology()

                'workspaceViewer.Open(openFileDialog1.FileName)
            Catch
                MessageBox.Show("Unable to load file " & openFileDialog1.FileName & ".")
                imageLoaded = False
                Return
            End Try

            imageLoaded = True

            ' check if its OK start a recognize at this point
            validateRecognize(0, 0)
            finalResults = Nothing
        End Sub

        ' user has de/selected a barcode Symbology set
        Private Sub barcodeSymbologyList_ItemCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckEventArgs) Handles barcodeSymbologyList.ItemCheck
            Dim index As Integer = e.Index
            Dim cs As CheckState = e.NewValue
            Dim map As SymbologyMap = CType(barcodeSymbologyList.Items(index), SymbologyMap)
            If Not map Is Nothing Then
                If cs = CheckState.Checked Then
                    options.Symbology = options.Symbology Or map.symbology
                Else
                    options.Symbology = options.Symbology And Not map.symbology
                End If
                ' this callback semantics are off in the sense that the controls
                ' count of checked items isn't updated until after all
                ' callbacks have been hit, which means that we can't tell
                ' how many are checked without adding a delta in.
                ' We'll let Validate handle the delta.
                If cs = CheckState.Checked Then
                    validateRecognize(1, 0)
                Else
                    validateRecognize(-1, 0)
                End If
            End If
        End Sub

        ' user has de/selected a scan direction
        Private Sub scanDirectionList_ItemCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckEventArgs) Handles scanDirectionList.ItemCheck
            Dim index As Integer = e.Index
            Dim cs As CheckState = e.NewValue
            Dim map As ScanDirectionMap = CType(scanDirectionList.Items(index), ScanDirectionMap)
            If Not map Is Nothing Then
                If cs = CheckState.Checked Then
                    options.Direction = options.Direction Or map.direction
                Else
                    options.Direction = options.Direction And Not map.direction
                End If
                ' this callback semantics are off in the sense that the controls
                ' count of checked items isn't updated until after all
                ' callbacks have been hit, which means that we can't tell
                ' how many are checked without adding a delta in.
                ' We'll let Validate handle the delta.
                If cs = CheckState.Checked Then
                    validateRecognize(0, 1)
                Else
                    validateRecognize(0, -1)
                End If
            End If
        End Sub

        ' Make sure that it is OK to proceed with recognition
        Private Sub validateRecognize(ByVal symbologyDeltaOnChangedCheck As Integer, ByVal scanDirectionDeltaOnChangedCheck As Integer)
            ' To be ready for recognition, an image must be loaded
            ' a symbology must be selected and a scan direction
            ' must be selected.
            recognizeButton.Enabled = imageLoaded AndAlso barcodeSymbologyList.CheckedItems.Count + symbologyDeltaOnChangedCheck <> 0 AndAlso scanDirectionList.CheckedItems.Count + scanDirectionDeltaOnChangedCheck <> 0
        End Sub

        Private Sub scanInterval_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles scanInterval.ValueChanged
            ' give feedback on the current value of the control
            scanIntervalLabel.Text = scanInterval.Value.ToString()
            If Not options Is Nothing Then
                options.ScanInterval = scanInterval.Value
            End If
        End Sub

        Private Sub expectedBarCodes_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles expectedBarCodes.ValueChanged
            ' give feedback on the current value of the control
            expectedBarcodesLabel.Text = expectedBarCodes.Value.ToString()
            options.ScanBarsToRead = expectedBarCodes.Value
        End Sub

        Private Sub workspaceViewer_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles workspaceViewer.Paint
            ' paint the frames on top of the existing image

            ' no results, nothing to do.
            If finalResults Is Nothing Then
                Return
            End If

            drawResultsPolygons(e.Graphics, finalResults)
        End Sub

        Private Sub drawResultsPolygons(ByVal g As System.Drawing.Graphics, ByVal results As BarCode())
            Dim penBlue As System.Drawing.Pen = New Pen(System.Drawing.Color.Blue, 4)
            Dim penOrange As System.Drawing.Pen = New Pen(System.Drawing.Color.Orange, 1)
            Dim zoom As Double = workspaceViewer.Zoom
            For i As Integer = 0 To results.Length - 1
                ' handle the bounding rectangles
                If showBoundingRects.Checked Then
                    Dim r As System.Drawing.Rectangle = results(i).BoundingRect
                    ' the bounding rects that come back can have negative values in
                    ' some cases.
                    If r.Width < 0 Then
                        r.X += r.Width
                        r.Width = -r.Width
                    End If
                    If r.Height < 0 Then
                        r.Y += r.Height
                        r.Height = -r.Height
                    End If
                    ' scale and offset the bounding rect.
                    r.X = CInt(Fix(r.X * zoom))
                    r.Y = CInt(Fix(r.Y * zoom))
                    r.Width = CInt(Fix(r.Width * zoom))
                    r.Height = CInt(Fix(r.Height * zoom))
                    r.Offset(workspaceViewer.ImagePosition)
                    g.DrawRectangle(penBlue, r)
                End If

                ' handle the bounding boxes (quadrilaterals)
                If showBoundingBoxes.Checked Then
                    Dim p1, p2, p3, p4 As System.Drawing.Point
                    p1 = scaleAndOffset(results(i).StartEdgePoints(0), zoom, workspaceViewer.ImagePosition)
                    p2 = scaleAndOffset(results(i).StartEdgePoints(1), zoom, workspaceViewer.ImagePosition)
                    p3 = scaleAndOffset(results(i).EndEdgePoints(1), zoom, workspaceViewer.ImagePosition)
                    p4 = scaleAndOffset(results(i).EndEdgePoints(0), zoom, workspaceViewer.ImagePosition)
                    g.DrawLine(penOrange, p1, p2)
                    g.DrawLine(penOrange, p2, p3)
                    g.DrawLine(penOrange, p3, p4)
                    g.DrawLine(penOrange, p4, p1)
                End If
            Next i
            penBlue.Dispose()
            penOrange.Dispose()
        End Sub

        Private Function scaleAndOffset(ByVal source As Point, ByVal zoom As Double, ByVal offset As Point) As Point
            ' scale and offset a point from image space into view space
            Dim dest As Point = New Point(source.X, source.Y)
            dest.X = CInt(Fix(dest.X * zoom))
            dest.Y = CInt(Fix(dest.Y * zoom))
            dest.Offset(offset.X, offset.Y)

            Return dest
        End Function

        Private Sub shrinkToFit_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles shrinkToFit.CheckedChanged
            If shrinkToFit.Checked Then
                workspaceViewer.AutoZoom = Atalasoft.Imaging.WinControls.AutoZoomMode.BestFitShrinkOnly
            Else
                workspaceViewer.AutoZoom = Atalasoft.Imaging.WinControls.AutoZoomMode.None
                workspaceViewer.Zoom = 1.0
            End If
        End Sub

        Private Sub showBoundingBoxes_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles showBoundingBoxes.CheckedChanged
            If Not finalResults Is Nothing Then
                workspaceViewer.Invalidate()
            End If
        End Sub

        Private Sub showBoundingRects_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles showBoundingRects.CheckedChanged
            If Not finalResults Is Nothing Then
                workspaceViewer.Invalidate()
            End If
        End Sub

        Private Sub aboutBtn_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles aboutBtn.Click
            Dim aboutBox As AtalaDemos.AboutBox.About = New AtalaDemos.AboutBox.About("About Atalasoft DotImage Barcode Reader Demo", "DotImage Barcode Reader Demo")
            aboutBox.Description = "The Barcode Reader Demo demonstrates how to read a barcode from an image.  This demo should be used to gain a basic understanding of how the DotImage Barcode recognition functions.  The demo allows you to set options, such as barcode types, scan directions, scan interval and the number of expected barcodes.  If you are having trouble recognizing a barcode, this demo may help to see why.  Requires DotImage and DotImage BarcodeReader."
            aboutBox.ShowDialog()
        End Sub

        Private Sub statusBox_DoubleClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles statusBox.DoubleClick
            Dim resForm As ResultForm = New ResultForm(statusBox.Text)
            Call resForm.ShowDialog()
        End Sub


        Private Sub btnMorphoNone_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMorphoNone.CheckedChanged
            If btnMorphoNone.Checked Then
                UpdateMorphology("None")
            End If
        End Sub

        Private Sub btnMorphoDilate_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMorphoDilate.CheckedChanged
            If btnMorphoDilate.Checked Then
                UpdateMorphology("Dilate")
            End If
        End Sub

        Private Sub btnMorphoErode_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMorphoErode.CheckedChanged
            If btnMorphoErode.Checked Then
                UpdateMorphology("Erode")
            End If
        End Sub

        Private Sub UpdateMorphology()
            ' call the default
            UpdateMorphology("None")
            Me.btnMorphoNone.Checked = True

        End Sub

        Private Sub UpdateMorphology(ByVal mode As String)
            ' load fresh COPY of the image
            If Me._tmpImg IsNot Nothing Then
                workspaceViewer.Image = DirectCast(Me._tmpImg.Clone(), AtalaImage)

                Select Case mode
                    Case "Dilate"
                        workspaceViewer.ApplyCommand(New MorphoDocumentCommand() With {.Mode = MorphoDocumentMode.Dilation, .ApplyToAnyPixelFormat = True})
                        Exit Select
                    Case "Erode"
                        workspaceViewer.ApplyCommand(New MorphoDocumentCommand() With {.Mode = MorphoDocumentMode.Erosion, .ApplyToAnyPixelFormat = True})
                        Exit Select
                End Select
            End If
        End Sub
    End Class
End Namespace
