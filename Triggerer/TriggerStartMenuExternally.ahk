#Persistent
#NoEnv
#SingleInstance Force
SetBatchLines, -1

PipeName := "\\.\pipe\DirectStartPipe"

SendPipeMessage("TRIGGER")

SendPipeMessage(Message)
{
    global PipeName

    hPipe := DllCall("CreateFile", "Str", PipeName, "UInt", 0x40000000, "UInt", 0, "UInt", 0, "UInt", 3, "UInt", 0, "UInt", 0)
    
    if (hPipe == -1)
    {
        ErrorLevel := DllCall("GetLastError")
        MsgBox, 16, Error, Could not open pipe. Check if DirectStart is running. Error: %ErrorLevel%
        return
    }

    VarSetCapacity(Buffer, StrLen(Message) + 1, 0)
    StrPut(Message, &Buffer, "UTF-8")
    BytesWritten := 0

    Result := DllCall("WriteFile", "UInt", hPipe, "Ptr", &Buffer, "UInt", StrLen(Message) * 2, "UIntP", BytesWritten, "UInt", 0) ; * 2 because UTF-8 encoding might require up to 2 bytes per character
    
    if (!Result)
    {
        ErrorLevel := DllCall("GetLastError")
        MsgBox, 16, Error, Could not write to pipe. Check if DirectStart is running. Error: %ErrorLevel%
    }
    else
    {

    }

    DllCall("CloseHandle", "UInt", hPipe)
}
