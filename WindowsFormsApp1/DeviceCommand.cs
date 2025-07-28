using System.Collections.Generic;

public static class DeviceCommand
{
    public static readonly Dictionary<string, byte[]> Commands = new Dictionary<string, byte[]>
    {
        { "TEST_COMMAND",    new byte[] { 0x00, 0x00 } },
        { "HOSTMACIP",       new byte[] { 0x00, 0x3F } },
        { "LED ON",      new byte[] {0x00, 0x10 } },
        { "LED OFF",    new byte[] {0x00, 0x11 } },
        { "ACK",    new byte[] {0x00, 0x0E } },
        { "GET ZEROS", new byte[] {0x00, 0x0A} },
        { "GET_ZEROS_RES", new byte[] {0x00, 0x0B} },
        { "Learn Cnf", new byte[] {0x00, 0x1C} },
        {"Get_Mac", new byte[] {0x00, 0x19 } },
        {"Get_Mac_Response", new byte[] {0x00, 0x1A } },
        {"Find_Last_and_Addr", new byte[] {0x00, 0x17 } },
        {"GET_SOCKETS", new byte[] {0x00, 0x08}},
        {"SOCKETS_RES",  new byte[] {0x00, 0x09}},
        {"Rlout_Low",  new byte[] {0x00, 0x14}},
        {"End_addressing", new byte[] {0x00, 0x1B} }


    };
}
