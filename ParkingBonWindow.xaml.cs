using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfParkingBon;

namespace ParkingBon;

/// <summary>
/// Interaction logic for ParkingBonWindow.xaml
/// </summary>
public partial class ParkingBonWindow : Window
{
    public static RoutedCommand mijnRouterCtrlN = new();
    public static RoutedCommand mijnRouterCtrlO = new();
    public static RoutedCommand mijnRouterCtrlS = new();
    public static RoutedCommand mijnRouterCtrlF2 = new();
    public static RoutedCommand mijnRouterClosure = new();
    public ParkingBonWindow()
    {
        InitializeComponent();
        CommandBinding mijnCtrlN = new CommandBinding(mijnRouterCtrlN, NewExecuted);
        this.CommandBindings.Add(mijnCtrlN);
        CommandBinding mijnCtrlO = new CommandBinding(mijnRouterCtrlO, openExecuted);
        this.CommandBindings.Add(mijnCtrlN);
        CommandBinding mijnCtrlS = new CommandBinding(mijnRouterCtrlO, saveExecuted);
        this.CommandBindings.Add(mijnCtrlS);
        CommandBinding mijnCtrlF2 = new CommandBinding(mijnRouterCtrlF2, printPreviewExecuted);
        this.CommandBindings.Add(mijnCtrlS);
        CommandBinding MijnClosure = new CommandBinding(mijnRouterClosure, closeExecuted);
        this.CommandBindings.Add(MijnClosure);

        KeyGesture toetsCtrlN = new KeyGesture(Key.N, ModifierKeys.Control);
        KeyBinding mijnKeyCtrlN = new KeyBinding(mijnRouterCtrlN, toetsCtrlN);
        this.InputBindings.Add(mijnKeyCtrlN);
        Nieuw();                
    }
    private void Nieuw()
    { 
        DatumBon.SelectedDate = DateTime.Now;
        AankomstLabelTijd.Content = DateTime.Now.ToLongTimeString();
        TeBetalenLabel.Content = "0 €";
        VertrekLabelTijd.Content = AankomstLabelTijd.Content;

        StatusItem.Content = "Nieuwe bon";
        SaveEnAfdruk(false);
    }
    private void SaveEnAfdruk(bool actief)
    {
        PrintPreviewButton.IsEnabled = actief;
        SaveButton.IsEnabled = actief;
        BonAfdrukken.IsEnabled = actief;
        BonOpslaaan.IsEnabled = actief;                
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (MessageBox.Show("Programma afsluiten ?", "Afsluiten", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
            e.Cancel = true;
    }

    private void minder_Click(object sender, RoutedEventArgs e)
    {
        int bedrag = Convert.ToInt32(TeBetalenLabel.Content.ToString().Replace(" €", ""));
        if (bedrag > 0)
            bedrag -= 1;
        TeBetalenLabel.Content = bedrag.ToString() + " €";
        VertrekLabelTijd.Content = Convert.ToDateTime(AankomstLabelTijd.Content).AddHours(0.5 * bedrag).ToLongTimeString();
        SaveEnAfdruk(!(bedrag == 0));
    }

    private void meer_Click(object sender, RoutedEventArgs e)
    {
        int bedrag = Convert.ToInt32(TeBetalenLabel.Content.ToString().Replace(" €", ""));
        DateTime vertrekuur = Convert.ToDateTime(AankomstLabelTijd.Content).AddHours(0.5 * bedrag);
        if (vertrekuur.Hour < 22)
            bedrag += 1;
        TeBetalenLabel.Content = bedrag.ToString() + " €";
        VertrekLabelTijd.Content = Convert.ToDateTime(AankomstLabelTijd.Content).AddHours(0.5 * bedrag).ToLongTimeString();
        SaveEnAfdruk(!(bedrag == 0));
    }
    private void nieuwAction(object sender, RoutedEventArgs e)
    {
        Nieuw();
    }
    private void closeAction(object sender, RoutedEventArgs e)
    {
        Close();
    }
    private void NewExecuted(object sender,ExecutedRoutedEventArgs e) {
        Nieuw();
    }    
    private void openExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        OpenFileDialog dlg = new OpenFileDialog();
        dlg.Filter = "Parkeerbonnen | *.bon";
        if (dlg.ShowDialog() == true)
        {
            using (StreamReader invoer = new StreamReader(dlg.FileName))
            {
                DatumBon.SelectedDate = Convert.ToDateTime(invoer.ReadLine());
                AankomstLabelTijd.Content = invoer.ReadLine();
                TeBetalenLabel.Content = invoer.ReadLine();
                VertrekLabelTijd.Content = invoer.ReadLine();
            }
            StatusItem.Content = dlg.FileName;
            SaveEnAfdruk(true);
        }
    }    
    private void saveExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        SaveFileDialog dlg = new SaveFileDialog();
        dlg.FileName = "*.bon";
        dlg.DefaultExt = ".bon";
        dlg.Filter = "Parkeerbonnen | *.txt";
        if (dlg.ShowDialog() == true)
        {
            using (StreamWriter bestand = new StreamWriter(dlg.FileName))
            {
                bestand.WriteLine(this.DatumBon.Text);
                bestand.WriteLine(this.TeBetalenLabel.Content);
                bestand.WriteLine(this.AankomstLabelTijd.Content);
                bestand.WriteLine(this.VertrekLabelTijd.Content);
            }
            StatusItem.Content = dlg.FileName;
        }
    }
    private double vertPositie;
    private void printPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        FixedDocument document = new FixedDocument();
        document.DocumentPaginator.PageSize = new Size(640, 320);
        PageContent inhoud = new PageContent();
        document.Pages.Add(inhoud);
        FixedPage pagina = new FixedPage();
        inhoud.Child = pagina;

        pagina.Width = 640;
        pagina.Height = 320;
        Image logo = new Image();
        logo.Source = logoImage.Source;
        logo.Margin = new Thickness(96);
        pagina.Children.Add(logo);
        vertPositie = 90;
        pagina.Children.Add(Regel("datum" + DatumBon.Text));
        pagina.Children.Add(Regel("starttijd : " + AankomstLabelTijd.Content));
        pagina.Children.Add(Regel("eindtijd" + VertrekLabelTijd.Content));
        pagina.Children.Add(Regel("bedrag betaald : " + TeBetalenLabel.Content));

        Afdrukvoorbeeld preview = new Afdrukvoorbeeld();
        preview.Owner = this;
        preview.AfdrukDocument = document;
        preview.ShowDialog();
    }
    private TextBlock Regel(string tekst)
    {
        TextBlock deRegel = new TextBlock();
        deRegel.Margin = new Thickness(300, vertPositie, 96, 96);
        deRegel.FontSize = 18;
        vertPositie += 36; 
        deRegel.Text = tekst;
        return deRegel;
    }
    private void closeExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        this.Close();
    }

    private void btnafsluiten_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
