using System;

public enum Action : byte
{
    OnBlock,
    OnResource,
    OnEmptySpace,
    OnGreenie
}

public enum Resource : byte
{
    CO2,
    H2,
    H2O,
    O2,
    Stone
}