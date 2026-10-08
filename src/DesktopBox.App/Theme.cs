using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Markup;
namespace DesktopBox;
internal static class Theme {
 internal static readonly Brush Text=new SolidColorBrush(Color.FromRgb(228,231,236));
 internal static readonly Brush Muted=new SolidColorBrush(Color.FromRgb(158,165,177));
 static Style Read(string xml)=>(Style)XamlReader.Parse(xml);
 internal static Style ButtonStyle=>Read("""
 <Style xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" TargetType="Button">
 <Setter Property="Foreground" Value="#E4E7EC"/><Setter Property="Background" Value="#10FFFFFF"/><Setter Property="BorderBrush" Value="#18FFFFFF"/><Setter Property="BorderThickness" Value="1"/><Setter Property="Cursor" Value="Hand"/>
 <Setter Property="Template"><Setter.Value><ControlTemplate TargetType="Button"><Border x:Name="Surface" Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="{TemplateBinding BorderThickness}" CornerRadius="7" Padding="{TemplateBinding Padding}"><ContentPresenter HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}" VerticalAlignment="Center"/></Border><ControlTemplate.Triggers><Trigger Property="IsMouseOver" Value="True"><Setter TargetName="Surface" Property="Background" Value="#22FFFFFF"/></Trigger><Trigger Property="IsPressed" Value="True"><Setter TargetName="Surface" Property="Background" Value="#30FFFFFF"/></Trigger></ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>
 """);
 internal static Style EditorStyle=>Read("""
 <Style xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" TargetType="TextBox"><Setter Property="Foreground" Value="#E4E7EC"/><Setter Property="Background" Value="#18FFFFFF"/><Setter Property="BorderBrush" Value="#40FFFFFF"/><Setter Property="CaretBrush" Value="#FFFFFF"/><Setter Property="Template"><Setter.Value><ControlTemplate TargetType="TextBox"><Border Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="1" CornerRadius="5" Padding="{TemplateBinding Padding}"><ScrollViewer x:Name="PART_ContentHost"/></Border></ControlTemplate></Setter.Value></Setter></Style>
 """);
 internal static Style SliderStyle=>Read("""
 <Style xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" TargetType="Slider"><Setter Property="MinHeight" Value="22"/><Setter Property="Template"><Setter.Value><ControlTemplate TargetType="Slider"><Grid Margin="4,0"><Track x:Name="PART_Track" Minimum="{TemplateBinding Minimum}" Maximum="{TemplateBinding Maximum}" Value="{TemplateBinding Value}" IsDirectionReversed="False"><Track.DecreaseRepeatButton><RepeatButton Command="Slider.DecreaseLarge"><RepeatButton.Template><ControlTemplate TargetType="RepeatButton"><Border Height="3" CornerRadius="2" Background="#8FFFFFFF"/></ControlTemplate></RepeatButton.Template></RepeatButton></Track.DecreaseRepeatButton><Track.IncreaseRepeatButton><RepeatButton Command="Slider.IncreaseLarge"><RepeatButton.Template><ControlTemplate TargetType="RepeatButton"><Border Height="3" CornerRadius="2" Background="#25FFFFFF"/></ControlTemplate></RepeatButton.Template></RepeatButton></Track.IncreaseRepeatButton><Track.Thumb><Thumb Width="12" Height="12" Cursor="Hand"><Thumb.Template><ControlTemplate TargetType="Thumb"><Ellipse Fill="#DCE2E9"/></ControlTemplate></Thumb.Template></Thumb></Track.Thumb></Track></Grid></ControlTemplate></Setter.Value></Setter></Style>
 """);
 internal static void Menu(ContextMenu menu){menu.Background=new SolidColorBrush(Color.FromRgb(36,40,48));menu.Foreground=Text;menu.BorderBrush=new SolidColorBrush(Color.FromArgb(40,255,255,255));menu.Padding=new Thickness(5);menu.Resources.Add(typeof(MenuItem),Read("""
 <Style xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" TargetType="MenuItem"><Setter Property="Foreground" Value="#E4E7EC"/><Setter Property="Padding" Value="12,8"/><Setter Property="Template"><Setter.Value><ControlTemplate TargetType="MenuItem"><Border x:Name="Surface" CornerRadius="5" Background="Transparent" Padding="{TemplateBinding Padding}"><ContentPresenter ContentSource="Header"/></Border><ControlTemplate.Triggers><Trigger Property="IsHighlighted" Value="True"><Setter TargetName="Surface" Property="Background" Value="#20FFFFFF"/></Trigger></ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>
 """));}
}

