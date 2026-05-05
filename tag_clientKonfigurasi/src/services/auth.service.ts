// src/services/auth.service.ts

import { clearToken, getAccessToken } from "@/helpers/token.helper"
import { authFetch } from "@/utils/fetcher"

interface LoginPayload {
  username: string
  password: string
}

interface ApiResponse<T> {
  metadata: {
    code: string
    message: string
  }
  response: T
}

interface LoginSuccess {
  token: string
  refreshToken: string
  expiresIn: number
}

// ===== TAMBAHAN UNTUK AUTH/ME =====

export interface UserInfo {
  fullName: string
  username: string
  avatar?: string
  cabang?: string
  group: string[]
  role: string
}

export interface AccessItem {
  subject: string
  action: string
}

export interface MenuItem {
  id?: string
  title?: string
  icon?: string
  href?: string
  navlabel?: boolean
  subheader?: string
  children?: MenuItem[]
}

export interface MeResponse {
  user: UserInfo
  acces: AccessItem[]
  Menu: MenuItem[]
}

const BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL

if (!BASE_URL) {
  throw new Error('NEXT_PUBLIC_API_BASE_URL belum diset')
}

export async function login(
  payload: LoginPayload
): Promise<LoginSuccess> {
  let res: Response

  try {
    res = await fetch(`${BASE_URL}Auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Modul': 'CONFIG', // 🔥 TAMBAH INI
      },
      body: JSON.stringify(payload),
    })
  } catch {
    // ❌ server mati / network error
    throw new Error('Server tidak dapat dihubungi')
  }

  let data: ApiResponse<LoginSuccess | ''>

  try {
    data = await res.json()
  } catch {
    // ❌ API tidak balikin JSON (HTML error, proxy error, dll)
    throw new Error('Respon server tidak valid')
  }

  /**
   * 🔴 ATURAN UTAMA
   * Backend adalah source of truth
   */
  if (!res.ok) {
    // contoh: 400, 401, 403, 500
    throw new Error(data?.metadata?.message || 'Terjadi kesalahan')
  }

  if (data.metadata.code !== '200') {
    throw new Error(data.metadata.message)
  }

  if (!data.response || !('token' in data.response)) {
    throw new Error('Token tidak ditemukan')
  }

  return data.response
}

export async function getMe(): Promise<MeResponse> {
  let res: Response;

  try {
    res = await authFetch(`${BASE_URL}Auth/me`, {
      method: "GET",
    });
  } catch (err) {
    throw new Error("Gagal mengambil data user");
  }

  let data: ApiResponse<MeResponse>;

  try {
    data = await res.json();
  } catch {
    throw new Error("Respon server tidak valid");
  }

  if (!res.ok || data.metadata.code !== "200") {
    throw new Error(data.metadata.message);
  }

  return data.response;
}

// export async function getMe(): Promise<MeResponse> {
//   // const token = localStorage.getItem('access_token')
//   const token = getAccessToken();

//   if (!token) {
//     throw new Error('Token tidak ditemukan')
//   }

//   let res: Response

//   try {
//     res = await fetch(`${BASE_URL}Auth/me`, {
//       headers: {
//         'Content-Type': 'application/json',
//         Authorization: `Bearer ${token}`,
//       },
//     })
//   } catch {
//     throw new Error('Server tidak dapat dihubungi')
//   }

//   let data: ApiResponse<MeResponse>

//   try {
//     data = await res.json()
//   } catch {
//     throw new Error('Respon server tidak valid')
//   }

//   if (!res.ok || data.metadata.code !== '200') {
//     throw new Error(data.metadata.message)
//   }

//   return data.response
// }

// src/services/auth.service.ts

export async function refreshToken(): Promise<{
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}> {
  const refresh = localStorage.getItem("refresh_token");
  if (!refresh) {
    throw new Error("No refresh token");
  }

  let res: Response;

  try {
    res = await fetch(
      `${process.env.NEXT_PUBLIC_API_BASE_URL}Auth/refresh`,
      {
        method: "POST",
        headers: {
          "X-Refresh-Token": refresh, // ✅ WAJIB
          "X-Modul": "CONFIG", 
          "Content-Type": "application/json",
        },
      }
    );
  } catch {
    throw new Error("Server tidak dapat dihubungi");
  }

  if (!res.ok) {
    throw new Error("Refresh token expired");
  }

  const data = await res.json();

  if (data.metadata.code !== "200") {
    throw new Error(data.metadata.message);
  }

  /**
   * 🔁 ROTATE TOKEN
   * refresh lama → MATI
   * refresh baru → SIMPAN
   */
  localStorage.setItem("refresh_token", data.response.refreshToken);

  return data.response;
}

export async function logout(): Promise<void> {
  const accessToken = getAccessToken();

  try {
    if (accessToken) {
      await fetch(
        `${process.env.NEXT_PUBLIC_API_BASE_URL}Auth/logout`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${accessToken}`,
            "Content-Type": "application/json",
          },
        }
      );
    }
  } catch {
    // ❗ tidak perlu throw
    // logout tetap jalan walau server error
  } finally {
    // 🔥 WAJIB: bersihkan client
    clearToken();
  }
}
