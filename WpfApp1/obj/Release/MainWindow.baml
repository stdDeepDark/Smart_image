   M S B A M L   `   `   `  ���� E  ASmartImage, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:WpfApp1WpfApp1      WpfApp1.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     'localclr-namespace:WpfApp1     -     � Title$  
SmartImage��$	��800��$
��1400�� � WindowStartupLocation$ CenterScreen=����	#727A7A7A � AllowsTransparency .  � WindowStyle$
 None=� � 	Resources  S� ObjectInstance!  System.Windows.Media.Colors S� 
MethodName   colorPropertiesOdp  	chkBullet �� 
TargetType �� Property	 �� Value
 �� 
TargetType �� CornerRadius ��Name T� X ��Name �� Triggers P� Property P� Value �� 
TargetName �� RelativeSource N� EnterActions �� 
TargetName �� TargetProperty	 W� To g� Duration N� ExitActions��%$  &	        &	      S� "  $ GetProperties���� " ��.���  �@d�	IsChecked$	 FalseY���  � ��
	 ���� ��  � ��	 �#999��  � ��	 �� "
 ��.R��� $��90��$��25��$ 10��#��z��._�� .+��� $ border��$��20��$��20��$ 9��$��Center=�$
��Left=�$	��1 0q�
�������z�T� $ 0Q��� $	 txt��#��z��$	��9 0q�$��Center=�
�������z�T� $ 18Q� P�  �@d�	IsChecked$
 trueY�. ��� $	 txt��	 �� *Tag+ �� *TemplatedParent+ � �� �� .���� .��W� $ border��$8 2(UIElement.RenderTransform).(TranslateTransform.X)�$ 70X�$ 	00:00:0.2D�W� $	 txt��$8 2(UIElement.RenderTransform).(TranslateTransform.X)�$ 0X�$ 	00:00:0.2D� �� .���� .��W� $ border��$8 2(UIElement.RenderTransform).(TranslateTransform.X)�$ 0X�$ 	00:00:0.2D�W� $	 txt��$8 2(UIElement.RenderTransform).(TranslateTransform.X)�$ 18X�$ 	00:00:0.2D�P�  �@d�	IsChecked$ FalseY�. ��� $	 txt��	 �� *	Content+ �� *TemplatedParent+ � ��.��� -    �Name$
 Grid�����	#00000000.+�� -   $ Grid2�����	#00000000$��0,0,0,0q�$��30��$	��Top=�.+��� -    ��Name$ title��$��Smart Image��$��30��$
��Left=�$	��Top=�$��16(�����  �� Color# � ���� -   $�� ��$��30��$��30��$	��Top=�$��Right=� J� RenderTransformOrigin$ 4.368,0.624&�$��0q�����  �� ImageSource$ 	close.png��� -   $ Grid1�����	#00000000$��0,30,0,0q�.+��� ���	#19000000$��1q�$��10,40,10,10q�._��� -    ��Name$ 
main_image��$��0,0,0,0q�$
��None=�$��Center=�$��Center=�c� -     c�Name$  toolBar1��$��30��$��0,0,0,0q�$	��Top=����	#33FFFFFF.@��� -	   ! ��Name$! mode_button��$��编辑模式��$
��Left=�$��Center=�$	��100��$��30��$��0,0,1,0q����	#66000000$��14(����	#19FFFFFF���� $ White���� -
   $! button1��$��选择图片��$
��Left=�$��Center=�$	��100��$��30��$��0,0,1,0q����	#66000000$��14(����	#19FFFFFF���� # � ���� -   $��显示所有文本框��$
��Left=�$��Center=�$	��120��$��30��$��0,0,1,0q����	#66000000$��14(����	#19FFFFFF���� # � ���� -   $! button_newlabel��$��新建文本框��$
��Left=�$��Center=�$	��100��$��30��$��0,0,1,0q����	#66000000$��14(����	#19FFFFFF���� # � ���� -   $! button_save��$��保存��$
��Left=�$��30��$��Center=�$��60��$��0,0,1,0q����	#66000000$��14(����	#19FFFFFF���� # � ���� -   $! button_delete��$��删除文本框��$	��100��$��30��$��0,0,1,0q����	#66000000$
��Left=�$��14(�$��Center=����	#19FFFFFF���� # � ���� -   $ label_fontColore��$��字体颜色��$��30��$��Center=�$��80��$
��Left=�$��Center=�$��Center=����	#66000000$��14(����	#19FFFFFF$��0,0,1,0q����� # � ���� -   " ��Name$" FontColor_ComboBox��$��30��$��Center=�$	��120��$ 0.863,0.615&����	#19FFFFFF���	#4CFFFFFF$
��Left=�$��0,0,1,0q�$��Center=�$��14(����� # �� Source## [   $ �� Opacity��%F   (��       * SystemColors.HighlightBrushKey+)�� $
 Blue��$$ 0Q����� .R��� % �� Orientation$% 
Horizontal=�.+��� $	��105������ *Name+�� ���� *Name+�� -   $ label_fontFamily��$��字体��$��30��$��Center=�$��50��$
��Left=�$��Center=�$��Center=����	#66000000$��14(����	#19FFFFFF$��0,0,1,0q����� # � ���� -   $" fontFamily_ComboBox��$��30��$��Center=�$	��120�����	#19FFFFFF���	#4CFFFFFF$��0,0,1,0q�$
��Left=�$��Center=�$��14(� ��%F   (��       * SystemColors.HighlightBrushKey+)�� $
 Blue��$$ 0Q��� -   $ label_fontSize��$��字号��$��30��$��Center=�$��50��$
��Left=�$��Center=�$��Center=����	#66000000$��14(����	#19FFFFFF$��0,0,1,0q����� # � ���� -   & ��Name$& fontSize_textBox��$��30��' �� TextWrapping$
' Wrap=�$��Center=�$��60��4  .System.Windows.Automation.AutomationProperties(   ItemType$( double��$ 2.343,0.538&����	#19FFFFFF$��0,0,1,0q�$
��Left=�$��Center=�) �� SelectionBrush
) ��·�$��14(�.������ $ 	#B2FFFFFF��.������ # � ��.���� -   $ label_borderColor��$��边框颜色��$��30��$��Center=�$��80��$
��Left=�$��Center=�$��Center=����	#66000000$��14(����	#19FFFFFF$��0,0,1,0q����� # � ���� -   $" BorderColor_ComboBox��$��30��$��Center=�$	��120��$ 0.863,0.615&����	#19FFFFFF���	#4CFFFFFF$
��Left=�$��0,0,1,0q�$��Center=�$��14(����� ## [   ��%F   (��       * SystemColors.HighlightBrushKey+)�� $
 Blue��$$ 0Q����� .R��� $% 
Horizontal=�.+��� $	��105������ *Name+�� ���� *Name+�� -   * ��Name$* check_backTrans��#��[ ���#5387b9+ d� 	IsChecked$+ FalseY�$��背景不透明��
, � Tag$, 背景透明��$��25��