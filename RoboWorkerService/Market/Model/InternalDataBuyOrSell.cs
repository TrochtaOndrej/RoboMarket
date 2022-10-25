namespace RoboWorkerService.Market.Model;

public record InternalData
{
    /// <summary> Strategie metody nakupu a prodeje </summary>
    public string Strategy { get; set; }

    /// <summary> Odkud se pocitala cena </summary>
    public decimal StartingPointPositionToBuyOrSell { get; set; }

    /// <summary> Interni cislovani orderu </summary>
    public int InternalNumber { get; set; }

    public int OrderFromInternalNumber { get; set; }
}