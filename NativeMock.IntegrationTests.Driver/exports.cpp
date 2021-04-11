#include "pch.h"

#define DllExport extern "C" __declspec( dllexport )

using ForwardHandler = int (__cdecl *)(int i);
ForwardHandler forward_handler;

DllExport int NmForward(int i)
{
	return forward_handler != nullptr
		? forward_handler(i)
		: 0;
}

DllExport void NmForwardSetHandler(ForwardHandler handler)
{
	forward_handler = handler;
}
