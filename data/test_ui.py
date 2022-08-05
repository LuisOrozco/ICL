import clr
clr.AddReference("Eto")
clr.AddReference("Rhino.UI")

from Rhino.UI import *
from Eto.Forms import *
from Eto.Drawing import *

dialog = Dialog()
dialog.Title = "Sample Eto Dialog"
dialog.Padding = Padding(5)

image_view = ImageView()
image_view.Image = Bitmap("C:\Users\ukeer\GitHub\ICL\data")
dialog.Content = image_view

dialog.ShowModal(RhinoEtoApp.MainWindow)