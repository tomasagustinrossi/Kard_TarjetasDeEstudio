namespace Tarjetas.App.Comportamientos;

/// <summary>El "temblor" clásico del modo edición: el elemento oscila apenas mientras <see cref="Activo"/> sea verdadero.</summary>
public sealed class TemblorBehavior : Behavior<VisualElement>
{
    public static readonly BindableProperty ActivoProperty = BindableProperty.Create(
        nameof(Activo), typeof(bool), typeof(TemblorBehavior), false,
        propertyChanged: (bindable, _, _) => ((TemblorBehavior)bindable).Actualizar());

    private VisualElement? vista;
    private CancellationTokenSource? cancelacion;

    public bool Activo
    {
        get => (bool)GetValue(ActivoProperty);
        set => SetValue(ActivoProperty, value);
    }

    protected override void OnAttachedTo(VisualElement bindable)
    {
        base.OnAttachedTo(bindable);
        vista = bindable;
        Actualizar();
    }

    protected override void OnDetachingFrom(VisualElement bindable)
    {
        Detener();
        vista = null;
        base.OnDetachingFrom(bindable);
    }

    private void Actualizar()
    {
        if (vista is null) return;
        if (Activo) Iniciar();
        else Detener();
    }

    private void Iniciar()
    {
        if (cancelacion is not null || vista is null) return;
        cancelacion = new CancellationTokenSource();
        _ = TemblarAsync(vista, cancelacion.Token);
    }

    private void Detener()
    {
        cancelacion?.Cancel();
        cancelacion?.Dispose();
        cancelacion = null;
        if (vista is null) return;
        vista.CancelAnimations();
        vista.Rotation = 0;
    }

    private static async Task TemblarAsync(VisualElement vista, CancellationToken ct)
    {
        try
        {
            await Task.Delay(Random.Shared.Next(0, 140), ct); // cada elemento arranca desfasado
            while (!ct.IsCancellationRequested)
            {
                await vista.RotateToAsync(1.4, 85, Easing.SinInOut);
                if (ct.IsCancellationRequested) break;
                await vista.RotateToAsync(-1.4, 85, Easing.SinInOut);
            }
        }
        catch (OperationCanceledException)
        {
            // al salir del modo edición
        }
    }
}
