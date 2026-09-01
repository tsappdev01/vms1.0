/****************************************************************************
* Copyright (C) 2017-2026 by Federal Authority for Identity, Citizenship, *
* Customs & Port Security (ICP).                                          *
*                                                                         *
* This file is part of UAE ID Card Toolkit.                               *
*                                                                         *
*   The Toolkit provides functionality to access UAE ID Card and          *
*   corresponding online services of ICP Validation Gateway (VG).         *
*                                                                         *
****************************************************************************/

/**
 * @file EIDAToolkit.h
 * @brief Main include file for the Emirates ID Card Toolkit.
 *
 * Include this single header to access all Toolkit types, error codes,
 * and API functions. It pulls in:
 * - EIDAToolkitTypes.h  -- Data types, structures, and enumerations
 * - EIDAToolkitError.h  -- Status/error code definitions
 * - EIDAToolkitApi.h    -- Public API function declarations
 */

#ifndef EIDA_TOOLKIT_H_
#define EIDA_TOOLKIT_H_

#if __GNUC__ >= 4
#pragma GCC visibility push(hidden)
#endif

#include <EIDAToolkitTypes.h>
#include <EIDAToolkitError.h>
#include <EIDAToolkitApi.h>

#if __GNUC__ >= 4
#pragma GCC visibility pop
#endif

#endif // EIDA_TOOLKIT_H_
