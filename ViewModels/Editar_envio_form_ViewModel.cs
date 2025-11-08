using Microsoft.EntityFrameworkCore;
using Proyecto_Isasi_Montanaro.Helpers;
using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

public class Editar_envio_form_ViewModel : INotifyPropertyChanged
{
    private readonly Envio _envioOriginal;
    private readonly ProyectoTallerContext _context;
    public Action? CloseAction { get; set; }


    public event PropertyChangedEventHandler? PropertyChanged;

    public Envio Envio { get; set; }
    public ObservableCollection<Estado> Estados { get; set; }

    private bool _isEditable;
    public bool IsEditable
    {
        get => _isEditable;
        set { _isEditable = value; OnPropertyChanged(); }
    }

    public ICommand ModificarCommand { get; }
    public ICommand CancelarCommand { get; }
    public ICommand GuardarCommand { get; }
    public ICommand VolverCommand { get; }

    public Editar_envio_form_ViewModel(Envio envioParam)
    {
        // Usar el contexto real
        _context = new ProyectoTallerContext();

        // Cargar la entidad real desde DB para que esté trackeada y tenga navegaciones
        var envioDb = _context.Envios
            .Include(e => e.IdEstadoNavigation)
            .FirstOrDefault(e => e.IdEnvio == envioParam.IdEnvio);

        if (envioDb == null)
        {
            MessageBox.Show("No se encontró el envío en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            // crear objeto mínimo para no romper bindings
            Envio = envioParam;
        }
        else
        {
            Envio = envioDb;
        }

        // Guardar copia para cancelar (clonamos solo los campos que nos interesan)
        _envioOriginal = new Envio
        {
            IdEnvio = Envio.IdEnvio,
            NumSeguimiento = Envio.NumSeguimiento,
            FechaDespacho = Envio.FechaDespacho,
            FechaEntrega = Envio.FechaEntrega,
            IdEstado = Envio.IdEstado
        };

        // Cargar lista de estados para el Combo
        Estados = new ObservableCollection<Estado>(_context.Estados.OrderBy(s => s.Nombre).ToList());

        // Commands
        ModificarCommand = new RelayCommand(_ => StartEditing());
        CancelarCommand = new RelayCommand(Cancelar); 
        VolverCommand = new RelayCommand(_ => CancelarEdicion());
        GuardarCommand = new RelayCommand(SaveChanges);

        IsEditable = false;
    }


    private void StartEditing()
    {
        IsEditable = true;
        // Forzar notificación para que los IsEnabled/Visibility se actualicen
        OnPropertyChanged(nameof(IsEditable));
    }

    private void Cancelar(object parameter) => CerrarVentana(parameter);

    private void CerrarVentana(object parameter)
    {
        if (parameter is Window window)
        {
            window.Close();
        }
    }
    private void CancelarEdicion()
    {
        Envio.NumSeguimiento = _envioOriginal.NumSeguimiento;
        Envio.FechaDespacho = _envioOriginal.FechaDespacho;
        Envio.FechaEntrega = _envioOriginal.FechaEntrega;
        Envio.IdEstado = _envioOriginal.IdEstado;

        OnPropertyChanged(nameof(Envio));
        OnPropertyChanged(nameof(FechaDespachoDT));
        OnPropertyChanged(nameof(FechaEntregaDT));

        IsEditable = false;
    }



    private void SaveChanges(object parameter)
    {
        try
        {
            // Validaciones mínimas (ejemplo: si requiere estado)
            // Ej: if (Envio.IdEstado == 0) { MessageBox.Show("Seleccioná un estado."); return; }

            // _context.Envios.Update(Envio); // no hace falta si lo cargaste desde el contexto, ya está trackeado
            _context.SaveChanges();

            MessageBox.Show("Cambios guardados correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            // Después de guardar volvemos a solo lectura
            IsEditable = false;

            // Refrescar originales (si querés que cancelar no vuelva al valor anterior al guardar)
            _envioOriginal.NumSeguimiento = Envio.NumSeguimiento;
            _envioOriginal.FechaDespacho = Envio.FechaDespacho;
            _envioOriginal.FechaEntrega = Envio.FechaEntrega;
            _envioOriginal.IdEstado = Envio.IdEstado;

            OnPropertyChanged();
            if (parameter is Window w)
                w.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    public DateTime? FechaDespachoDT
    {
        get => Envio?.FechaDespacho?.ToDateTime(TimeOnly.MinValue);
        set
        {
            if (value.HasValue)
                Envio.FechaDespacho = DateOnly.FromDateTime(value.Value);
            else
                Envio.FechaDespacho = null;
            OnPropertyChanged();
        }
    }

    public DateTime? FechaEntregaDT
    {
        get => Envio?.FechaEntrega?.ToDateTime(TimeOnly.MinValue);
        set
        {
            if (value.HasValue)
                Envio.FechaEntrega = DateOnly.FromDateTime(value.Value);
            else
                Envio.FechaEntrega = null;
            OnPropertyChanged();
        }
    }


    private void OnPropertyChanged(string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}