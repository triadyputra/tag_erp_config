"use client";

import React, { useEffect, useState } from "react";
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
import { useComboCabang, useComboGroup } from "@/hooks/useComboGroup";
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
  FullName: "",
  Photo: "",
  PhoneNumber: "",
  Cabang: "",
  Group: [],
  Active: true,
};

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

  useEffect(() => {
    if (akunToEdit) {
      setValues({
        ...emptyValues,
        ...akunToEdit,
        Group: akunToEdit.Group ?? [],
        Active: akunToEdit.Active ?? true,
      });
    } else {
      setValues(emptyValues);
    }
  }, [akunToEdit]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // =========================
    // VALIDASI
    // =========================
    if (!values.FullName?.trim()) {
      showSnackbar("Nama lengkap wajib diisi", "warning");
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
    } catch (err: any) {
      showSnackbar(err.message || "Gagal menyimpan akun", "error");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={!!akunToEdit} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogHeader
        title={values.Id ? "Edit Akun" : "Tambah Akun"}
        subtitle="Pengisian dan pengelolaan informasi user"
        statusLabel={values.Id ? "EDIT" : "CREATE"}
        statusColor={values.Id ? "info" : "warning"}
      />

      <Divider />

      <DialogContent>
        <Box component="form" id="form-akun" mt={3} onSubmit={handleSubmit}>
          <Grid container spacing={3}>

            <Grid size={{ xs: 12 }}>
              <FormLabel>Nama Lengkap</FormLabel>
              <TextField
                fullWidth
                size="small"
                value={values.FullName}
                onChange={(e) =>
                  setValues({ ...values, FullName: e.target.value })
                }
              />
            </Grid>

            <Grid size={{ xs: 12 }}>
              <FormLabel>Username</FormLabel>
              <TextField
                fullWidth
                size="small"
                value={values.UserName}
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
                value={values.Email}
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
                getOptionLabel={(o) => o.title ?? ""}
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
        💡 Cabang kosong user bisa akses semua cabang
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