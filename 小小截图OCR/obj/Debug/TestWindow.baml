   M S B A M L   `   `   `  ����J  F小小截图OCR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:小小截图OCR小小截图OCR  5   	        小小截图OCR.TestWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   /localclr-namespace:小小截图OCR     5   	     � Title$  
TestWindow��5   	   $	��300��6   $	��300��6)   .��5
      � -   5         �Name$
 grid��5
      
������ ��5      �� 5       �� 
TargetType" �6    �� Triggers 5      P� 5       P� Property �@J�IsMouseOver P� Value . 5      5      .+�5         小小截图OCR.Tip  -   5          Name$	 tip��5         PlacementTarget 6   ��  �� ElementName$
 grid����5      �� 5      "  6   .�5      �� 	 �� Property	 � y�
 �� Value$
 Visible=�6$    5      �� 5       �� Binding 6   �� *SelectState+$ editor�� �� Value Selected6H   .a�5   	   �� 	 � y�$
 Hidden=�6&   5   	   5         Child. 5      �� $	��200��6   $	��100��6   $	��123��6(   ���	#C0000000
�������6_   5       �� 5$      -   5        $��Button��5$      $
��Left=�6   $��	10,10,0,0q�67   $	��Top=�6J   $��75��6b   6m   5%      5&      