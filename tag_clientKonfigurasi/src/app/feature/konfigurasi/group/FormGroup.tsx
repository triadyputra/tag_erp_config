'use client';

import React, { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Dialog,
  DialogContent,
  FormLabel,
  TextField,
  CircularProgress,
  Grid,
  Divider,
  DialogActions,
  Typography,
  Select,
  MenuItem,
} from '@mui/material';
import NestedAccessCheckbox from './NestedAccessCheckbox';
import { GroupList } from '@/app/(DashboardLayout)/types/feature/konfigurasi/group';
import DialogHeader from '@/app/components/DialogHeader/DialogHeader';
import { IconDeviceFloppy } from '@tabler/icons-react';
import { useComboModul } from '@/hooks/useComboGroup';
import { fetchAccessRoles } from '@/services/akun/group.service';

interface FormGroupProps {
  groupToEdit: GroupList | null;
  onClose: () => void;
  onSubmit: (payload: GroupList) => Promise<void>;
}

const emptyGroup: GroupList = {
  Id: '',
  Name: '',
  Access: '',
  Keterangan: '',
  Photo: '',
  IdModul: '',
};

const FormGroup: React.FC<FormGroupProps> = ({
  groupToEdit,
  onClose,
  onSubmit,
}) => {
  const { moduls, loading: loadingModul } = useComboModul();
  const [values, setValues] = useState<GroupList>(emptyGroup);
  const [accessList, setAccessList] = useState<
    { IdController: string; IdAction: string }[]
  >([]);
  const [accessRoles, setAccessRoles] = useState<any[]>([]);
  const [loadingAccess, setLoadingAccess] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [errors, setErrors] = useState<{
    IdModul?: string;
    Name?: string;
    Access?: string;
    Keterangan?: string;
  }>({});

  /* =======================
   * INIT FORM
   * ======================= */
  useEffect(() => {
    if (!groupToEdit) {
        setValues(emptyGroup);
        setAccessList([]);
        return;
    }

    setValues({
        Id: groupToEdit.Id ?? '',
        Name: groupToEdit.Name ?? '',
        Access: groupToEdit.Access ?? '',
        Keterangan: groupToEdit.Keterangan ?? '',
        Photo: groupToEdit.Photo ?? '',
        IdModul: groupToEdit.IdModul ?? '',
        NamaModul: groupToEdit.NamaModul,
    });

    try {
        const parsed =
        groupToEdit.Access?.startsWith('[')
            ? JSON.parse(groupToEdit.Access)
            : [];
        setAccessList(parsed);
    } catch {
        setAccessList([]);
    }
	}, [groupToEdit]);

  useEffect(() => {
    if (!groupToEdit) {
      setAccessRoles([]);
      return;
    }

    let active = true;

    async function loadAccessRoles() {
      try {
        setLoadingAccess(true);
        const data = await fetchAccessRoles();
        if (active) {
          setAccessRoles(Array.isArray(data) ? data : []);
        }
      } catch (err) {
        console.error(err);
        if (active) setAccessRoles([]);
      } finally {
        if (active) setLoadingAccess(false);
      }
    }

    loadAccessRoles();

    return () => {
      active = false;
    };
  }, [groupToEdit]);

  const validate = () => {
    const newErrors: typeof errors = {};

    if (!values.IdModul || values.IdModul.trim() === '') {
      newErrors.IdModul = 'Modul wajib dipilih';
    }

    if (!values.Name || values.Name.trim() === "") {
      newErrors.Name = "Nama Group wajib diisi";
    } else if (values.Name.trim().length < 3) {
      newErrors.Name = "Minimal 3 karakter";
    }

    if (!accessList || accessList.length === 0) {
      newErrors.Access = "Minimal pilih 1 hak akses";
    }

    if (values.Keterangan && values.Keterangan.length > 100) {
      newErrors.Keterangan = "Maksimal 100 karakter";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  /* =======================
   * SUBMIT
   * ======================= */
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validate() || isSubmitting) return;

    try {
      setIsSubmitting(true);

      const payload: GroupList = {
        ...values,
        Access: JSON.stringify(accessList),
      };

      await onSubmit(payload);
      onClose();
    } catch (err: any) {
      console.error(err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={!!groupToEdit} onClose={onClose} maxWidth="md" fullWidth>
      <DialogHeader
        title={values.Id ? "Edit Group" : "Tambah Group"}
        subtitle="Pengisian dan pengelolaan informasi Group pengguna"
        statusLabel={values.Id ? "EDIT" : "CREATE"}
        statusColor={values.Id ? "info" : "warning"}
      />

      <Divider />
      <form onSubmit={handleSubmit}>
        <DialogContent>
          <Box mt={3}>
            <Grid container spacing={3}>
              {/* MODUL */}
              <Grid
                size={{
                  xs: 12,
                  lg: 12,
                }}
              >
                <FormLabel>Modul</FormLabel>
                <Select
                  fullWidth
                  size="small"
                  displayEmpty
                  value={values.IdModul}
                  error={!!errors.IdModul}
                  disabled={loadingModul}
                  onChange={(e) => {
                    setValues({ ...values, IdModul: e.target.value });
                    if (errors.IdModul) {
                      setErrors({ ...errors, IdModul: undefined });
                    }
                  }}
                >
                  <MenuItem value="" disabled>
                    {loadingModul ? 'Memuat modul...' : 'Pilih modul'}
                  </MenuItem>
                  {moduls.map((modul) => (
                    <MenuItem key={modul.value} value={modul.value}>
                      {modul.title} ({modul.value})
                    </MenuItem>
                  ))}
                </Select>
                {errors.IdModul && (
                  <Typography color="error" fontSize={12} mt={0.5}>
                    {errors.IdModul}
                  </Typography>
                )}
              </Grid>

              {/* NAMA GROUP */}
              <Grid
                size={{
                  xs: 12,
                  lg: 12,
                }}
              >
                <FormLabel>Nama Group</FormLabel>
                <TextField
                  size="small"
                  fullWidth
                  value={values.Name}
                  error={!!errors.Name}
                  helperText={errors.Name}
                  onChange={(e) => {
                    setValues({ ...values, Name: e.target.value });

                    if (errors.Name) {
                      setErrors({ ...errors, Name: undefined });
                    }
                  }}
                />
              </Grid>

              {/* KETERANGAN */}
              <Grid
                size={{
                  xs: 12,
                  lg: 12,
                }}
              >
                <FormLabel>Keterangan</FormLabel>
                <TextField
                  size="small"
                  fullWidth
                  value={values.Keterangan}
                  error={!!errors.Keterangan}
                  helperText={errors.Keterangan}
                  onChange={(e) => {
                    setValues({ ...values, Keterangan: e.target.value });
                    if (errors.Keterangan) {
                      setErrors({ ...errors, Keterangan: undefined });
                    }
                  }}
                />
              </Grid>

              {/* ACCESS CHECKBOX */}
              <Grid
                size={{
                  xs: 12,
                  lg: 12,
                }}
              >
                {loadingAccess ? (
                  <Box display="flex" justifyContent="center" py={2}>
                    <CircularProgress size={24} />
                  </Box>
                ) : (
                  <NestedAccessCheckbox
                    key={groupToEdit?.Id || 'new'}
                    data={accessRoles}
                    selected={accessList}
                    onChange={setAccessList}
                  />
                )}
              </Grid>
              
              {errors.Access && (
                <Typography color="error" fontSize={12}>
                  {errors.Access}
                </Typography>
              )}
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
            💡 Atur hak akses sesuai peruntukannya
          </Typography>

          <Box display="flex" gap={1}>
            <Button
              sx={{ mr: 1 }}
              variant="outlined"
              onClick={onClose}
              disabled={isSubmitting}
            >
              Batal
            </Button>

            <Button
              type="submit"
              variant="contained"
              disabled={isSubmitting}
              startIcon={
                isSubmitting ? (
                  <CircularProgress size={18} color="inherit" />
                ) : (
                  <IconDeviceFloppy size={16} />
                )
              }
            >
              {isSubmitting ? "Menyimpan..." : "Simpan"}
            </Button>
          </Box>
        </DialogActions>
      </form>
      
      
    </Dialog>
  );
};

export default FormGroup;
