// src/services/akun/group.service.ts
import { API_BASE_URL } from '@/config/api.config';
import { authFetch } from '@/utils/fetcher';
import { GroupList } from '@/app/(DashboardLayout)/types/feature/konfigurasi/group';

const ROLE_URL = `${API_BASE_URL}Role`;
const ACCESS_ROLE_URL = `${API_BASE_URL}Role/accesRole`;
const ROLE_LIST_URL = `${API_BASE_URL}Role/GetListRole`;

interface FetchGroupParams {
  filter?: string;
  page: number;
  pageSize: number;
}

export async function fetchGroups(
  params: FetchGroupParams
) {
  const query = new URLSearchParams({
    filter: params.filter ?? '',
    page: params.page.toString(),
    pageSize: params.pageSize.toString(),
  }).toString();

  const res = await authFetch(`${ROLE_LIST_URL}?${query}`);
  const json = await res.json();

  if (!res.ok) {
    throw new Error(json?.message || 'Gagal mengambil data group');
  }

  return json;
}

export async function fetchAccessRoles(modul?: string) {
  const query = modul
    ? `?modul=${encodeURIComponent(modul)}`
    : '';
  const res = await authFetch(`${ACCESS_ROLE_URL}${query}`);
  const json = await res.json();

  if (!res.ok) {
    throw new Error(json?.message || 'Gagal mengambil access role');
  }

  return json;
}

export async function saveGroup(payload: GroupList) {
  const isEdit = Boolean(payload.Id);

  const res = await authFetch(
    isEdit ? `${ROLE_URL}/${payload.Id}` : ROLE_URL,
    {
      method: isEdit ? 'PUT' : 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    }
  );

  const json = await res.json();
  if (!res.ok) {
    throw new Error(json?.message || 'Gagal menyimpan group');
  }

  return json;
}

export async function deleteGroup(id: string) {
  const res = await authFetch(`${ROLE_URL}/${id}`, {
    method: 'DELETE',
  });

  const json = await res.json();
  if (!res.ok) {
    throw new Error(json?.message || 'Gagal menghapus group');
  }

  return json;
}
