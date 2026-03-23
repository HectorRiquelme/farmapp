namespace FarmApp.Domain.Models;

public enum TipoFarmacia
{
    Turno,
    Urgencia,
    NoDefinido
}

public enum EstadoApertura
{
    AbiertaAhora,
    PosiblementeAbierta,
    HorarioNoConfirmado,
    Cerrada,
    SinDatos
}

public enum FuenteBusqueda
{
    Api,
    Cache,
    SinResultados
}
