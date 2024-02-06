using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using System.Drawing;
using System.IO;
using ReactiveUI;
using ShadowLink.Views;
using ShadowLink.Models;

namespace ShadowLink.ViewModels;

public class MainViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public ICommand UploadImageCommand { get; set; }
    public ICommand ExtractMessageCommand { get; set; }
    public Window ParentWindow { get; }
    
    private Bitmap? _bitmap;
    private string _userInput;

    public string UserInputEmbedText
    {
        get =>
            _userInput;
        set =>
            this.RaiseAndSetIfChanged(ref _userInput,
                value);
    }

    public ICommand EmbedMessageCommand { get; }

    public MainViewModel()
    {
        UploadImageCommand = ReactiveCommand.Create(UploadImage);
        EmbedMessageCommand = ReactiveCommand.Create(EmbedMessage);
        ExtractMessageCommand = ReactiveCommand.Create(ExtractMessage);
    }

    private async Task UploadImage()
    {
        var dialog = new OpenFileDialog();
        dialog.Filters.Add(new FileDialogFilter()
        {
            Name = "Image",
            Extensions =
            {
                "jpg",
                "png",
                "bmp"
            }
        });

        var result = await dialog.ShowAsync(this.ParentWindow);

        if (result != null)
        {
            using (var stream = File.OpenRead(result[0])) // Open the file stream
            {
                _bitmap = new Bitmap(stream); // Store the image in the '_bitmap' variable
            }
        }
    }

    

    public Bitmap? EmbedMessage()
    {

        if (_bitmap != null && !string.IsNullOrEmpty(_userInput))
        {
            // Assuming ImageCipher is a static class and EmbedText is a static method
            // Also assuming EmbedText takes a Bitmap and a string as parameters
            return ImageCipher.EmbedText(_userInput,
                _bitmap);
        }
        throw new Exception("No image uploaded or no message provided");
    }


    public void ExtractMessage()
    {
        Debug.WriteLine("extract clicked");
        // Get the bitmap from memory
        // Call the ExtractText method from the ImageCipher.cs file
        // Display the extracted message in the text box
    }

#pragma warning restore CA1822 // Mark members as static
}