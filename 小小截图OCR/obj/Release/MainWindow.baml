   M S B A M L   `   `   `  ���� J  F小小截图OCR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:小小截图OCR小小截图OCR       小小截图OCR.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     /localclr-namespace:小小截图OCR     -     � Title$  
MainWindow��$	��350��$	��525�� � ShowInTaskbar .   � WindowStyle$
 None=� � WindowState$ 	Maximized=� � Topmost .  � 
ResizeMode$ CanMinimize=�$y�Visible=� � DataContext ��  �� RelativeSource �� *Self+ � 	Resources ��%    	 J� InputBindings	 �� 

 �� Key$	
 Esc�� �� Command �� *HideCommand+ �� CommandParameter �� .��� -    �Name$
 grid��.+�!   小小截图OCR.ImageEditor  -     Name$ editor��!   小小截图OCR.ImageEditor   	MouseMove$ editor_MouseMove��   Click$ editor_Click��   AcceptSelect$ editor_AcceptSelect��   Close$ editor_Close��   CancelSelect$ editor_CancelSelect��   SelectorBorderBrush
 �����   小小截图OCR.Tip     PlacementTarget ��  �� ElementName$
 grid������  �� 
TargetType"  .���  �� Property � y� �� Value$ Hidden=� �� Triggers ��  �� Binding �� *SelectState+$ editor�� �� Value	 Fixed.a���  � y�$ Visible=���  �� *SelectState+$ editor��
 Resize.a���  � y�$ Visible=�   Child. ��  �� Orientation$ Vertical=�.+�#   小小截图OCR.PixelObserver  -     Name$ pixelObserver��$	��155��$	��105��
�������$��2q�    BitmapSource  �� *BackgroundBitmap+�� -   ! ��Name$! tipSize�����	#C0000000
�������" �� Padding$" 5 5 0 0q���`� # `� ConverterParameter$
#  × ��$ `� 	Converter$    小小截图OCR.Lisp  *" (concat (add $0 1) $ (add $1 1))+.1��� % �� Path$% SelectWidth��� $% SelectHeight��� -   $! tipRGB�����	#C0000000
�������$" 5 0 0 0q���`� $   &   
Expression$+& %(format 'RGB:({0},{1},{2})' $1 $2 $3)��.1��� $% SelectColor.R��� $% SelectColor.G��� $% SelectColor.B�