"use client";

import React, { useEffect, useRef, useState } from "react";
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  FormLabel,
  MenuItem,
  Select,
  TextField,
  CircularProgress,
  Divider,
  Typography,
  Grid,
} from "@mui/material";
import Autocomplete from "@mui/material/Autocomplete";

import { AkunList } from "@/app/(DashboardLayout)/types/feature/konfigurasi/akun";
import {
  useComboCabang,
  useComboGroup,
  getFilterMasterKtp,
  getDetailMasterKtp,
  MasterKtpOption,
} from "@/hooks/useComboGroup";
import DialogHeader from "@/app/components/DialogHeader/DialogHeader";
import { saveAkun } from "@/services/akun/akun.service";
import { useSnackbar } from "@/app/context/SnackbarContext";
import { IconDeviceFloppy } from "@tabler/icons-react";

interface FormAkunProps {
  akunToEdit?: AkunList | null;
  onClose: () => void;
  onSaved?: () => void;
}

const emptyValues: AkunList = {
  Id: "",
  UserName: "",
  Email: "",
  NoKtp: "",
  FullName: "",
  Photo: "",
  PhoneNumber: "",
  Cabang: "",
  Group: [],
  Active: true,
};

/** API/list boleh mengembalikan null — TextField tidak boleh value={null} */
function normalizeAkunForm(source: Partial<AkunList>): AkunList {
  return {
    ...emptyValues,
    ...source,
    Id: source.Id ?? "",
    UserName: source.UserName ?? "",
    Email: source.Email ?? "",
    NoKtp: source.NoKtp ?? "",
    FullName: source.FullName ?? "",
    Photo: source.Photo ?? "",
    PhoneNumber: source.PhoneNumber ?? "",
    Cabang: source.Cabang ?? "",
    Group: source.Group ?? [],
    Active: source.Active ?? true,
  };
}

const FormAkun: React.FC<FormAkunProps> = ({
  akunToEdit,
  onClose,
  onSaved,
}) => {
  const { showSnackbar } = useSnackbar();
  const { groups, loading } = useComboGroup();
  const { cabang: cabangOptions, loading: loadingCabang } = useComboCabang();

  const [values, setValues] = useState<AkunList>(emptyValues);
  const [submitting, setSubmitting] = useState(false);

  const [keyword, setKeyword] = useState("");
  const [list, setList] = useState<MasterKtpOption[]>([]);
  const [showDropdown, setShowDropdown] = useState(false);
  const [selected, setSelected] = useState<MasterKtpOption | null>(null);
  const [loadingSearch, setLoadingSearch] = useState(false);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const karyawanRef = useRef<HTMLDivElement>(null);

  const isEdit = Boolean(values.Id);
  const isNewAkun = !isEdit;
  /** Akun lama tanpa No KTP — boleh edit nama manual */
  const isLegacyAkun = isEdit && !values.NoKtp?.trim();
  const fullNameFromMaster = Boolean(values.NoKtp?.trim());

  useEffect(() => {
    if (akunToEdit) {
      const next = normalizeAkunForm(akunToEdit);
      setValues(next);
      if (akunToEdit.NoKtp) {
        setSelected({
          NOKTP: akunToEdit.NoKtp,
          NAMALENGKAP: akunToEdit.FullName || "",
        });
        setKeyword(akunToEdit.FullName || "");
      } else {
        setSelected(null);
        setKeyword(akunToEdit.FullName || "");
      }
    } else {
      setValues(emptyValues);
      setSelected(null);
      setKeyword("");
      setList([]);
    }
  }, [akunToEdit]);

  useEffect(() => {
    if (selected) return;
    if (!keyword) {
      setList([]);
      return;
    }

    let active = true;
    setLoadingSearch(true);

    const timeout = setTimeout(async () => {
      try {
        if (keyword.length < 3) {
          if (active) setList([]);
          return;
        }
        const res = await getFilterMasterKtp(keyword);
        if (active) setList(res || []);
      } catch {
        if (active) setList([]);
      } finally {
        if (active) setLoadingSearch(false);
      }
    }, 400);

    return () => {
      active = false;
      clearTimeout(timeout);
    };
  }, [keyword, selected]);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (
        karyawanRef.current &&
        !karyawanRef.current.contains(e.target as Node)
      ) {
        setShowDropdown(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const openKaryawanPicker = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (loadingDetail) return;
    setShowDropdown(true);
  };

  const handleSelectKaryawan = async (item: MasterKtpOption) => {
    try {
      if (loadingDetail) return;
      setSelected(item);
      setKeyword(item.NAMALENGKAP);
      setShowDropdown(false);
      setLoadingDetail(true);

      const kdCabangFromList = item.KDCABANG?.trim();
      if (kdCabangFromList) {
        setValues((v) => ({ ...v, Cabang: kdCabangFromList }));
      }

      const detail = await getDetailMasterKtp(item.NOKTP);
      const kdCabang = (detail.KDCABANG ?? item.KDCABANG ?? "").trim();
      setValues((v) => ({
        ...v,
        NoKtp: detail.NOKTP,
        FullName: detail.NAMALENGKAP,
        ...(kdCabang ? { Cabang: kdCabang } : {}),
      }));
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Gagal memuat data KTP";
      showSnackbar(msg, "error");
    } finally {
      setLoadingDetail(false);
    }
  };

  const resetKaryawan = () => {
    setSelected(null);
    setKeyword(values.FullName || "");
    setList([]);
    setValues((v) => ({ ...v, NoKtp: "" }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (isNewAkun && !values.NoKtp?.trim()) {
      showSnackbar("Pilih karyawan dari Master KTP (cari berdasarkan nama)", "warning");
      return;
    }

    if (!values.UserName?.trim()) {
      showSnackbar("Username wajib diisi", "warning");
      return;
    }

    if (values.UserName.length < 3) {
      showSnackbar("Username minimal 3 karakter", "warning");
      return;
    }

    if (/\s/.test(values.UserName)) {
      showSnackbar("Username tidak boleh mengandung spasi", "warning");
      return;
    }

    if (!values.Group || values.Group.length === 0) {
      showSnackbar("Group wajib dipilih", "warning");
      return;
    }

    try {
      setSubmitting(true);
      await saveAkun(values);
      showSnackbar("Akun berhasil disimpan", "success");
      onSaved?.();
      onClose();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Gagal menyimpan akun";
      showSnackbar(msg, "error");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={!!akunToEdit} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogHeader
        title={values.Id ? "Edit Akun" : "Tambah Akun"}
        subtitle={
          isLegacyAkun
            ? "Akun lama: nama bisa diedit manual. Pilih karyawan untuk mengaitkan No KTP (opsional)."
            : "No KTP dan nama lengkap dari Master KTP (HRD)"
        }
        statusLabel={values.Id ? "EDIT" : "CREATE"}
        statusColor={values.Id ? "info" : "warning"}
      />

      <Divider />

      <DialogContent sx={{ overflow: "visible" }}>
        <Box component="form" id="form-akun" mt={3} onSubmit={handleSubmit}>
          <Grid container spacing={3}>
            <Grid size={{ xs: 12 }}>
              <FormLabel>Karyawan (Master KTP)</FormLabel>
              <Box ref={karyawanRef} sx={{ position: "relative" }}>
                <Box
                  role="button"
                  tabIndex={0}
                  onClick={openKaryawanPicker}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" || e.key === " ") {
                      e.preventDefault();
                      setShowDropdown(true);
                    }
                  }}
                  sx={(theme) => ({
                    border: `1px solid ${theme.palette.divider}`,
                    borderRadius: 1,
                    px: 2,
                    py: 1.5,
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    cursor: loadingDetail ? "wait" : "pointer",
                    "&:hover": {
                      borderColor: theme.palette.primary.main,
                      backgroundColor: theme.palette.action.hover,
                    },
                  })}
                >
                  <Box>
                    <Typography fontSize={12} color="text.secondary">
                      Nama
                    </Typography>
                    <Typography fontSize={14} fontWeight={600}>
                      {loadingDetail
                        ? "Mengambil data..."
                        : keyword ||
                          (isLegacyAkun
                            ? "Pilih karyawan (opsional)"
                            : "Pilih karyawan")}
                    </Typography>
                  </Box>
                  {selected && (
                    <Box
                      component="span"
                      onClick={(e) => {
                        e.stopPropagation();
                        resetKaryawan();
                      }}
                      sx={{
                        fontSize: 16,
                        px: 1,
                        cursor: "pointer",
                        color: "text.secondary",
                        "&:hover": { color: "error.main" },
                      }}
                    >
                      ✕
                    </Box>
                  )}
                </Box>

                {showDropdown && (
                  <Box
                    sx={{
                      position: "absolute",
                      top: "100%",
                      left: 0,
                      right: 0,
                      bgcolor: "background.paper",
                      border: 1,
                      borderColor: "divider",
                      borderRadius: 2,
                      mt: 1,
                      zIndex: 1400,
                      boxShadow: 3,
                    }}
                    onMouseDown={(e) => e.stopPropagation()}
                  >
                    <Box p={1}>
                      <TextField
                        fullWidth
                        size="small"
                        placeholder="Cari nama karyawan..."
                        autoFocus
                        value={keyword}
                        onChange={(e) => setKeyword(e.target.value)}
                        onMouseDown={(e) => e.stopPropagation()}
                        onClick={(e) => e.stopPropagation()}
                      />
                    </Box>
                    <Box maxHeight={220} overflow="auto">
                      {loadingSearch ? (
                        <Box p={2} textAlign="center">
                          <CircularProgress size={20} />
                        </Box>
                      ) : keyword.length < 3 ? (
                        <Box p={2}>
                          <Typography fontSize={12} color="text.secondary">
                            Ketik minimal 3 huruf...
                          </Typography>
                        </Box>
                      ) : list.length === 0 ? (
                        <Box p={2}>
                          <Typography fontSize={13}>
                            Data tidak ditemukan
                          </Typography>
                        </Box>
                      ) : (
                        list.map((item) => (
                          <Box
                            key={item.NOKTP}
                            onMouseDown={(e) => e.preventDefault()}
                            onClick={(e) => {
                              e.stopPropagation();
                              handleSelectKaryawan(item);
                            }}
                            sx={{
                              p: 1.5,
                              cursor: "pointer",
                              borderBottom: 1,
                              borderColor: "divider",
                              "&:hover": { bgcolor: "action.hover" },
                            }}
                          >
                            <Typography fontWeight={600} fontSize={13}>
                              {item.NAMALENGKAP}
                            </Typography>
                            <Typography
                              variant="caption"
                              color="text.secondary"
                              display="block"
                            >
                              {item.KELAMIN || "-"}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {item.NOKTP}
                              {item.KDCABANG ? ` • ${item.KDCABANG}` : ""}
                            </Typography>
                          </Box>
                        ))
                      )}
                    </Box>
                  </Box>
                )}
              </Box>
            </Grid>

            <Grid size={{ xs: 12 }}>
              <FormLabel>No KTP</FormLabel>
              <TextField
                fullWidth
                size="small"
                value={values.NoKtp ?? ""}
                disabled={fullNameFromMaster}
                placeholder={
                  fullNameFromMaster
                    ? "Dari Master KTP"
                    : "Belum diisi — pilih karyawan untuk mengaitkan"
                }
              />
            </Grid>

            <Grid size={{ xs: 12 }}>
              <FormLabel>Nama Lengkap</FormLabel>
              <TextField
                fullWidth
                size="small"
                value={values.FullName ?? ""}
                disabled={fullNameFromMaster}
                onChange={(e) =>
                  setValues({ ...values, FullName: e.target.value })
                }
                placeholder={
                  fullNameFromMaster
                    ? "Dari Master KTP"
                    : isLegacyAkun
                      ? "Opsional — kosong akan memakai username"
                      : "Isi nama lengkap"
                }
              />
            </Grid>

            <Grid size={{ xs: 12 }}>
              <FormLabel>Username</FormLabel>
              <TextField
                fullWidth
                size="small"
                value={values.UserName ?? ""}
                onChange={(e) =>
                  setValues({ ...values, UserName: e.target.value })
                }
              />
            </Grid>

            <Grid size={{ xs: 12 }}>
              <FormLabel>Email</FormLabel>
              <TextField
                fullWidth
                size="small"
                value={values.Email ?? ""}
                onChange={(e) =>
                  setValues({ ...values, Email: e.target.value })
                }
              />
            </Grid>

            <Grid size={{ xs: 12 }}>
              <FormLabel>Cabang</FormLabel>
              <Autocomplete
                options={cabangOptions ?? []}
                loading={loadingCabang}
                getOptionLabel={(o) => o.title ?? o.value ?? ""}
                value={
                  cabangOptions?.find((c) => c.value === values.Cabang) ?? null
                }
                onChange={(_, v) =>
                  setValues({
                    ...values,
                    Cabang: v?.value ?? "",
                  })
                }
                renderInput={(params) => (
                  <TextField {...params} size="small" />
                )}
              />
            </Grid>

            <Grid size={{ xs: 12 }}>
              <FormLabel>Group</FormLabel>
              <Autocomplete
                multiple
                options={groups || []}
                loading={loading}
                getOptionLabel={(o) => o.title}
                value={(groups || []).filter((g) =>
                  (values.Group || []).includes(g.value)
                )}
                onChange={(_, v) =>
                  setValues({
                    ...values,
                    Group: v.map((x) => x.value),
                  })
                }
                renderInput={(params) => (
                  <TextField {...params} size="small" />
                )}
              />
            </Grid>

            <Grid size={{ xs: 12 }}>
              <FormLabel>Status</FormLabel>
              <Select
                fullWidth
                size="small"
                value={values.Active ? "true" : "false"}
                onChange={(e) =>
                  setValues({
                    ...values,
                    Active: e.target.value === "true",
                  })
                }
              >
                <MenuItem value="true">Aktif</MenuItem>
                <MenuItem value="false">Non Aktif</MenuItem>
              </Select>
            </Grid>
          </Grid>
        </Box>
      </DialogContent>

      <DialogActions
        sx={{
          px: 3,
          py: 2,
          borderTop: (theme) => `1px solid ${theme.palette.divider}`,
          display: "flex",
          justifyContent: "space-between",
        }}
      >
        <Typography
          fontSize={12}
          sx={(theme) => ({
            display: "inline-flex",
            alignItems: "center",
            gap: 0.5,
            px: 1.5,
            py: 0.5,
            borderRadius: 1,
            backgroundColor:
              theme.palette.mode === "dark"
                ? theme.palette.primary.dark
                : theme.palette.primary.light,
            color:
              theme.palette.mode === "dark"
                ? theme.palette.primary.contrastText
                : theme.palette.primary.main,
            border: `1px solid ${
              theme.palette.mode === "dark"
                ? theme.palette.primary.main
                : theme.palette.primary.light
            }`,
            fontStyle: "italic",
          })}
        >
          {isLegacyAkun
            ? "Akun lama tanpa No KTP — nama opsional (kosong = username)"
            : "Pilih karyawan dari Master KTP HRD"}
        </Typography>

        <Box display="flex" gap={1}>
          <Button variant="outlined" onClick={onClose}>
            Batal
          </Button>

          <Button
            type="submit"
            form="form-akun"
            variant="contained"
            disabled={submitting}
            startIcon={
              submitting ? (
                <CircularProgress size={18} color="inherit" />
              ) : (
                <IconDeviceFloppy size={16} />
              )
            }
          >
            {submitting ? "Menyimpan..." : "Simpan"}
          </Button>
        </Box>
      </DialogActions>
    </Dialog>
  );
};

export default FormAkun;
