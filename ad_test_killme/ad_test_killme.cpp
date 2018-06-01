// ad_test_killme.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "activeds.h"
#include "atlbase.h"

#pragma comment(lib, "Activeds.lib")
#pragma comment(lib, "Adsiid.lib")

const int FETCH_NUM = 1;




int TestLogin()
{
	CComPtr<IADsContainer> pCont;


	HRESULT hr = ADsGetObject( L"LDAP://CN=users,DC=forest,DC=internal", 
		IID_IADsContainer, 
		(void**) &pCont );

	if ( !SUCCEEDED(hr) )
	{
		return 0;
	}


	CComPtr<IADs> pObject;
	LPWSTR szUsername = NULL;
	LPWSTR szPassword = NULL;

	// Insert code to securely retrieve the user name and password.

	hr = ADsOpenObject(L"LDAP://rootDSE",
		L"FOREST\\Test",
		L"$Power321",
		ADS_SECURE_AUTHENTICATION, 
		IID_IADs,
		(void**) &pObject);
	return 1;
}

int TestGrops()
{
	CComPtr<IADsUser> pIADsUser;
	HRESULT hr = ADsGetObject(L"WinNT://FOREST/Test1,user", IID_IADsUser, (void**) &pIADsUser);
	if(pIADsUser)
	{
		BSTR bstr;
		VARIANT var;
		hr = pIADsUser->get_Department(&bstr);
		if(hr == S_OK)
		{
			SysFreeString(bstr);
		}


		CComPtr<IADsMembers> pGroups;
		hr = pIADsUser->Groups(&pGroups);
		IUnknown *pUnk;
		hr = pGroups->get__NewEnum(&pUnk);

		IEnumVARIANT *pEnum;
		hr = pUnk->QueryInterface(IID_IEnumVARIANT,(void**)&pEnum);
		if (FAILED(hr)) return hr;

		pUnk->Release();



		IADs *pADs;
		ULONG lFetch;
		IDispatch *pDisp;

		VariantInit(&var);
		hr = pEnum->Next(1, &var, &lFetch);
		while(hr == S_OK)
		{
			if (lFetch == 1)
			{
				pDisp = V_DISPATCH(&var);
				pDisp->QueryInterface(IID_IADs, (void**)&pADs);
				pADs->get_Name(&bstr);
				printf("Group belonged: %S\n",bstr);
				SysFreeString(bstr);
				pADs->Release();
			}
			VariantClear(&var);
			pDisp=NULL;
			hr = pEnum->Next(1, &var, &lFetch);
		};
		hr = pEnum->Release();
	}
	return 1;
}

int TestSearchDir()
{
	HRESULT hr;
	///////////////IDirectorySearch///////////////////////////////////////////////////////////
	CComPtr<IDirectorySearch> pDSSearch;

	hr = ADsGetObject( L"LDAP://DC=forest,DC=internal", 
		IID_IDirectorySearch, 
		(void**) &pDSSearch );

	if ( !SUCCEEDED(hr) )
	{
		return 0;
	}

	LPWSTR pszAttr[] = { L"description", L"Name", L"distinguishedname" };
	ADS_SEARCH_HANDLE hSearch;
	DWORD dwCount = 0;
	ADS_SEARCH_COLUMN col;
	DWORD dwAttrNameSize = sizeof(pszAttr)/sizeof(LPWSTR);

	// Search for all objects with the 'cn' property that start with TESTCOMP.
	hr = pDSSearch->ExecuteSearch(L"(&(objectClass=user)(objectCategory=person)(cn=TESTCOMP))",pszAttr ,dwAttrNameSize,&hSearch );

	LPWSTR pszColumn;
	while( pDSSearch->GetNextRow( hSearch) != S_ADS_NOMORE_ROWS )
	{
		// Get the property.
		hr = pDSSearch->GetColumn( hSearch, L"distinguishedname", &col );

		// If this object supports this attribute, display it.
		if ( SUCCEEDED(hr) )
		{ 
			if (col.dwADsType == ADSTYPE_CASE_IGNORE_STRING)
				wprintf(L"The description property:%s\r\n", col.pADsValues->CaseIgnoreString); 
			pDSSearch->FreeColumn( &col );
		}
		else
			puts("description property NOT available");
		puts("------------------------------------------------");
		dwCount++;
	}
	pDSSearch->CloseSearchHandle(hSearch);
	///////////////IDirectorySearch///////////////////////////////////////////////////////////
	return 1;
}
int TestOUEnem()
{
	HRESULT hr;
	///////////////IDirectorySearch///////////////////////////////////////////////////////////
	CComPtr<IDirectorySearch> pDSSearch;


	hr = ADsGetObject( L"LDAP://DC=forest,DC=internal", 
		IID_IDirectorySearch, 
		(void**) &pDSSearch );

	if ( !SUCCEEDED(hr) )
	{
		return 0;
	}

	LPWSTR pszAttr[] = { L"description", L"Name", L"distinguishedname" };
	ADS_SEARCH_HANDLE hSearch;
	DWORD dwCount = 0;
	ADS_SEARCH_COLUMN col;
	DWORD dwAttrNameSize = sizeof(pszAttr)/sizeof(LPWSTR);

	    // Only search for direct child objects of the container.
    ADS_SEARCHPREF_INFO rgSearchPrefs[3];
    rgSearchPrefs[0].dwSearchPref = ADS_SEARCHPREF_SEARCH_SCOPE;
    rgSearchPrefs[0].vValue.dwType = ADSTYPE_INTEGER;
    rgSearchPrefs[0].vValue.Integer = ADS_SCOPE_ONELEVEL;

    // Set the page size.
    rgSearchPrefs[2].dwSearchPref = ADS_SEARCHPREF_PAGESIZE;
    rgSearchPrefs[2].vValue.dwType = ADSTYPE_INTEGER;
    rgSearchPrefs[2].vValue.Integer = 1000;

	hr = pDSSearch->SetSearchPreference(rgSearchPrefs, ARRAYSIZE(rgSearchPrefs));
    if(FAILED(hr))
    {
        return 0;
    }
	// Search for all objects with the 'cn' property that start with TESTCOMP.
	hr = pDSSearch->ExecuteSearch(L"(&(objectCategory=*)(objectClass=organizationalUnit))",pszAttr ,dwAttrNameSize,&hSearch );
	
    if(FAILED(hr))
    {
        return 0;
    }

	LPWSTR pszColumn;
	while( pDSSearch->GetNextRow( hSearch) != S_ADS_NOMORE_ROWS )
	{
		// Get the property.
		hr = pDSSearch->GetColumn( hSearch, L"distinguishedname", &col );

		// If this object supports this attribute, display it.
		if ( SUCCEEDED(hr) )
		{ 
			if (col.dwADsType == ADSTYPE_CASE_IGNORE_STRING)
				wprintf(L"The description property:%s\r\n", col.pADsValues->CaseIgnoreString); 
			pDSSearch->FreeColumn( &col );
		}
		else
			puts("description property NOT available");
		puts("------------------------------------------------");
		dwCount++;
	}
	pDSSearch->CloseSearchHandle(hSearch);
	///////////////IDirectorySearch///////////////////////////////////////////////////////////
	return 1;
}
int _tmain(int argc, char* argv[])
{
	CoInitialize(NULL);
	
	TestOUEnem();
	/*TestLogin();
	TestSearchDir();
	TestGrops();*/
	CoUninitialize();
	return 0;
}