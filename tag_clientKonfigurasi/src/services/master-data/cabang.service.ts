import { API_BASE_URL } from '@/config/api.config';
import { parseApiResponse } from '@/helpers/global.helper';
import { authFetch } from '@/utils/fetcher';

// ===============================
// URL
// ===============================
const CABANG_URL = `${API_BASE_URL}MstCabang`;
const CABANG_LIST_URL = `${API_BASE_URL}MstCabang/GetListCabang`;
const CABANG_UPLOAD_URL = `${API_BASE_URL}MstCabang/Upload`;
const CABANG_TEMPLATE_URL = `${API_BASE_URL}MstCabang/DownloadTemplate`;

// ===============================
// PARAMS
// ===============================
interface FetchCabangParams {
  filter?: string;
  page: number;
  pageSize: number;
}

// ===============================
// GET LIST CABANG
// ===============================
export async function fetchCabang(params: FetchCabangParams) {
  const query = new URLSearchParams({
    filter: params.filter ?? '',
    page: params.page.toString(),
    pageSize: params.pageSize.toString(),
  }).toString();

  const res = await authFetch(`${CABANG_LIST_URL}?${query}`);
  return parseApiResponse(res);
}

// ===============================
// GET DETAIL CABANG
// ===============================
export async function fetchCabangById(kode: string) {
  const res = await authFetch(`${CABANG_URL}/${kode}`);
  return parseApiResponse(res);
}

// ===============================
// CREATE / UPDATE CABANG
// ===============================
export async function saveCabang(payload: any) {
  const isEdit = Boolean(payload.KdCabang);

  const res = await authFetch(CABANG_URL, {
    method: isEdit ? 'PUT' : 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });

  return parseApiResponse(res);
}

// ===============================
// DELETE CABANG
// ===============================
export async function deleteCabang(kode: string) {
  const res = await authFetch(`${CABANG_URL}/${kode}`, {
    method: 'DELETE',
  });

  return parseApiResponse(res);
}

// ===============================
// DOWNLOAD TEMPLATE EXCEL
// ===============================
export async function downloadTemplateCabang() {
  const res = await authFetch(CABANG_TEMPLATE_URL);

  if (!res.ok) throw new Error('Gagal download template');

  const blob = await res.blob();
  const url = window.URL.createObjectURL(blob);

  const a = document.createElement('a');
  a.href = url;
  a.download = 'Template_MasterCabang.xlsx';
  a.click();

  window.URL.revokeObjectURL(url);
}

// ===============================
// UPLOAD EXCEL CABANG
// ===============================
export async function uploadCabang(file: File) {
  const formData = new FormData();
  formData.append('file', file);

  const res = await authFetch(CABANG_UPLOAD_URL, {
    method: 'POST',
    body: formData,
  });

  // ⚠️ response bisa JSON atau file (error excel)
  const contentType = res.headers.get('content-type');

  if (contentType?.includes('application/json')) {
    return parseApiResponse(res);
  }

  // kalau file (error excel)
  const blob = await res.blob();
  const url = window.URL.createObjectURL(blob);

  const a = document.createElement('a');
  a.href = url;
  a.download = `Upload_MasterCabang_Error.xlsx`;
  a.click();

  window.URL.revokeObjectURL(url);

  return {
    success: false,
    message: 'Terdapat error pada upload. Silakan cek file.',
  };
}