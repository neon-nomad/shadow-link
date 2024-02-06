using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ShadowLink.ViewModels;

namespace ShadowLink.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}