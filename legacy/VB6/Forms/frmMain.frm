VERSION 5.00
Begin VB.Form frmMain 
   Caption         =   "Tela Principal"
   ClientHeight    =   7500
   ClientLeft      =   5130
   ClientTop       =   2520
   ClientWidth     =   11580
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   ScaleHeight     =   7500
   ScaleWidth      =   11580
   Begin VB.Menu mnuRegistry 
      Caption         =   "Cadastro Geral"
      Begin VB.Menu mnuRegistry1 
         Caption         =   "Customers"
         Index           =   0
      End
      Begin VB.Menu mnuRegistry1 
         Caption         =   "Products"
         Index           =   1
      End
   End
End
Attribute VB_Name = "frmMain"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit
Private Enum MainMenu
    Customers = 0
    Products
End Enum

Private Sub Form_Load()
    Me.Caption = "Sistema de Consulta - API VB6"
End Sub

Private Sub mnuRegistry1_Click(Index As Integer)
    Select Case Index
        Case Customers
            ShowForm frmCustomers
        Case Products
            ShowForm frmProductAPI
    End Select
End Sub

Private Sub ShowForm(frm As Form)
    On Error Resume Next
    If frm.Visible = False Then
        frm.Show vbModal
    Else
        frm.ZOrder 0
    End If
End Sub
