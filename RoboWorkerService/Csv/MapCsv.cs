using System.Globalization;
using AutoMapper;
using ExchangeSharp;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Csv;

public sealed class AutoMapperProfileCsv : Profile
{
    public  AutoMapperProfileCsv()
    {
        CreateMap<TransactionData, TransactionData>();
    }
}
