#include "pch.h"

#include <algorithm>
#include <map>
#include <string>
#include <shared_mutex>

#define DllExport extern "C" __declspec( dllexport )

using GetProcAddressFunction = FARPROC(__stdcall*)(HMODULE, LPCSTR);

typedef std::shared_mutex Lock;
typedef std::unique_lock<Lock> WriteLock;
typedef std::shared_lock<Lock> ReadLock;
typedef std::basic_string<wchar_t> String;

bool hook_enabled = false;

Lock proxies_lock;
std::map<std::pair<String, String>, void*> proxies{};

Lock module_names_lock;
wchar_t module_name_buffer[1024];
std::map<HMODULE, String> module_names{};

String parse_string_from_char(const char* data)
{
	const auto len = strlen(reinterpret_cast<char const*>(data));
	String wc(len, L' ');

	size_t result;
	mbstowcs(&wc[0], data, len);
	return wc;
}

String resolve_module_name(HMODULE hModule)
{
	{
		ReadLock read_lock(module_names_lock);
		auto existing_entry = module_names.find(hModule);
		if (existing_entry != module_names.end())
			return existing_entry->second;
	}

	WriteLock write_lock(module_names_lock);
	auto existing_entry = module_names.find(hModule);
	if (existing_entry != module_names.end())
		return existing_entry->second;

	auto result = GetModuleFileNameW(hModule, reinterpret_cast<LPWSTR>(&module_name_buffer), sizeof module_name_buffer);
	if (result == 0 || result == sizeof module_name_buffer)
		return String{};

	auto c = L'\\';
	auto i = result - 1;
	for(; i > 0; i--)
		if (module_name_buffer[i] == c)
			break;
	i++;

	auto moduleName = String{ reinterpret_cast<LPWSTR>(&module_name_buffer[i]), result - i};
	std::transform(moduleName.begin(), moduleName.end(), moduleName.begin(), towlower);

	return moduleName;
}

FARPROC __stdcall GetProcAddressHook(HMODULE hModule, LPCSTR lpProcName)
{
	if (!hook_enabled)
		return GetProcAddress(hModule, lpProcName);

	// Ignore ordinal lookups
	auto module_name = resolve_module_name(hModule);

	String function_name;
	if ((reinterpret_cast<size_t>(lpProcName) & ~0xFFFF) == 0)
	{
		auto formatted_ordinal = std::to_string(reinterpret_cast<size_t>(lpProcName));
		function_name = String(formatted_ordinal.length() + 1, L'#' );
		mbstowcs(&function_name[1], formatted_ordinal.c_str(), formatted_ordinal.length());
	}
	else
	{
		function_name = parse_string_from_char(lpProcName);
	}

	ReadLock read_lock(proxies_lock);
	if (!hook_enabled)
		return GetProcAddress(hModule, lpProcName);

	auto existing_proxy = proxies.find(std::make_pair(module_name, function_name));
	if (existing_proxy != proxies.end())
		return static_cast<FARPROC>(existing_proxy->second);

	return GetProcAddress(hModule, lpProcName);
}

DllExport GetProcAddressFunction NmInitGetProcAddressHook()
{
	hook_enabled = true;
	return &GetProcAddressHook;
}

DllExport void NmRegisterProxy(wchar_t const* moduleNamePtr, wchar_t const* functionNamePtr, void* nativePtr)
{
	WriteLock write_lock(proxies_lock);

	auto moduleName = String{moduleNamePtr, wcslen(moduleNamePtr)};
	std::transform(moduleName.begin(), moduleName.end(), moduleName.begin(), towlower);
	auto functionName = String{functionNamePtr, wcslen(functionNamePtr)};

	proxies.insert_or_assign(std::make_pair(moduleName, functionName), nativePtr);
}

DllExport void NmDestroyProxy()
{
	WriteLock write_lock(proxies_lock);

	hook_enabled = false;
	proxies.clear();
}
