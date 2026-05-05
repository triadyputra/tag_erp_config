// src/services/akun/akun.service.ts

import { API_BASE_URL } from '@/config/api.config';
import { parseApiResponse } from '@/helpers/global.helper';
import { authFetch } from '@/utils/fetcher';

// ===============================
// URL
// ===============================
const AKUN_URL = `${API_BASE_URL}Akun`;
const AKUN_LIST_URL = `${API_BASE_URL}Akun/GetListAkun`;

// ===============================
// GET LIST AKUN
// ===============================
interface FetchAkunParams {
  filter?: string;
  page: number;
  pageSize: number;
}

export async function fetchAkun(params: FetchAkunParams) {
  const query = new URLSearchParams({
    filter: params.filter ?? '',
    page: params.page.toString(),
    pageSize: params.pageSize.toString(),
  }).toString();

  const res = await authFetch(`${AKUN_LIST_URL}?${query}`);
  return parseApiResponse(res);
}

// ===============================
// GET DETAIL AKUN
// ===============================
export async function fetchAkunById(id: string) {
  const res = await authFetch(`${AKUN_URL}/${id}`);
  return parseApiResponse(res);
}

// ===============================
// CREATE / UPDATE AKUN
// ===============================
export async function saveAkun(payload: any) {
  const isEdit = Boolean(payload.Id);

  const res = await authFetch(
    isEdit ? `${AKUN_URL}/${payload.Id}` : AKUN_URL,
    {
      method: isEdit ? 'PUT' : 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    }
  );

  return parseApiResponse(res);
}

// ===============================
// DELETE AKUN
// ===============================
export async function deleteAkun(id: string) {
  const res = await authFetch(`${AKUN_URL}/${id}`, {
    method: 'DELETE',
  });

  return parseApiResponse(res);
}