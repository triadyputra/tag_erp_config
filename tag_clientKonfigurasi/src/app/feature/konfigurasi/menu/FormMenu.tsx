"use client";

import React, { useEffect, useState } from "react";
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
  IconButton,
  Paper,
  Stack,
} from "@mui/material";
import DialogHeader from "@/app/components/DialogHeader/DialogHeader";
import { IconPlus, IconTrash, IconDeviceFloppy } from "@tabler/icons-react";

interface FormMenuProps {
  menuToEdit: any | null;
  onClose: () => void;
  onSubmit: (payload: any) => Promise<void>;
}

const emptyMenu = {
  IdMenu: "",
  NamaMenu: "",
  NoUrut: 0,
  IdModul: "",
  Icon: "",
  ParentId: "",
  Controllers: [],
};

const FormMenu: React.FC<FormMenuProps> = ({
  menuToEdit,
  onClose,
  onSubmit,
}) => {
  const [values, setValues] = useState<any>(emptyMenu);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errors, setErrors] = useState<any>({});

  useEffect(() => {
    if (!menuToEdit) {
      setValues(emptyMenu);
      return;
    }

    setValues({
      IdMenu: menuToEdit.IdMenu ?? "",
      NamaMenu: menuToEdit.NamaMenu ?? "",
      NoUrut: menuToEdit.NoUrut ?? 0,
      IdModul: menuToEdit.IdModul ?? "",
      Icon: menuToEdit.Icon ?? "",
      ParentId: menuToEdit.ParentId ?? "",
      Controllers: menuToEdit.Controllers ?? [],
    });
  }, [menuToEdit]);

  const validate = () => {
    const err: any = {};

    if (!values.IdMenu) err.IdMenu = "ID Menu wajib diisi";
    if (!values.NamaMenu) err.NamaMenu = "Nama menu wajib diisi";
    if (!values.IdModul) err.IdModul = "Modul wajib diisi";
    if (!values.Controllers || values.Controllers.length === 0)
      err.Controllers = "Minimal 1 controller";

    setErrors(err);
    return Object.keys(err).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validate() || isSubmitting) return;

    try {
      setIsSubmitting(true);
      await onSubmit(values);
      onClose();
    } finally {
      setIsSubmitting(false);
    }
  };

  const addController = () => {
    setValues({
      ...values,
      Controllers: [
        ...values.Controllers,
        {
          IdController: "",
          NamaController: "",
          Url: "",
          Icon: "",
          NoUrut: 0,
          Actions: [],
        },
      ],
    });
  };

  const removeController = (index: number) => {
    const updated = [...values.Controllers];
    updated.splice(index, 1);
    setValues({ ...values, Controllers: updated });
  };

  const addAction = (cIndex: number) => {
    const updated = [...values.Controllers];
    updated[cIndex].Actions.push({
      IdAction: "",
      NamaAction: "",
      NoUrut: 0,
    });

    setValues({ ...values, Controllers: updated });
  };

  const removeAction = (cIndex: number, aIndex: number) => {
    const updated = [...values.Controllers];
    updated[cIndex].Actions.splice(aIndex, 1);
    setValues({ ...values, Controllers: updated });
  };

  return (
    <Dialog open={!!menuToEdit} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogHeader
        title={values.IdMenu ? "Edit Menu" : "Tambah Menu"}
        subtitle="Pengaturan Menu, Controller, dan Action"
        statusLabel={values.IdMenu ? "EDIT" : "CREATE"}
        statusColor={values.IdMenu ? "info" : "warning"}
      />

      <Divider />

      <form onSubmit={handleSubmit}>
        <DialogContent>
          <Box mt={2}>
            <Grid container spacing={3}>

              {/* ID MENU */}
              <Grid size={{ xs: 12 }}>
                <FormLabel>ID Menu</FormLabel>
                <TextField
                  size="small"
                  fullWidth
                  value={values.IdMenu}
                  error={!!errors.IdMenu}
                  helperText={errors.IdMenu}
                  onChange={(e) =>
                    setValues({ ...values, IdMenu: e.target.value })
                  }
                />
              </Grid>

              {/* NAMA MENU */}
              <Grid size={{ xs: 12 }}>
                <FormLabel>Nama Menu</FormLabel>
                <TextField
                  size="small"
                  fullWidth
                  value={values.NamaMenu}
                  error={!!errors.NamaMenu}
                  helperText={errors.NamaMenu}
                  onChange={(e) =>
                    setValues({ ...values, NamaMenu: e.target.value })
                  }
                />
              </Grid>

              {/* MODUL */}
              <Grid size={{ xs: 12, md: 6 }}>
                <FormLabel>Id Modul</FormLabel>
                <TextField
                  size="small"
                  fullWidth
                  value={values.IdModul}
                  error={!!errors.IdModul}
                  helperText={errors.IdModul}
                  onChange={(e) =>
                    setValues({ ...values, IdModul: e.target.value })
                  }
                  placeholder="HRD / SIMRS"
                />
              </Grid>

              {/* ICON MENU */}
              <Grid size={{ xs: 12, md: 6 }}>
                <FormLabel>Icon Menu</FormLabel>
                <TextField
                  size="small"
                  fullWidth
                  value={values.Icon}
                  onChange={(e) =>
                    setValues({ ...values, Icon: e.target.value })
                  }
                  placeholder="IconHome"
                />
              </Grid>

              {/* PARENT */}
              <Grid size={{ xs: 12 }}>
                <FormLabel>Parent Menu (optional)</FormLabel>
                <TextField
                  size="small"
                  fullWidth
                  value={values.ParentId}
                  onChange={(e) =>
                    setValues({ ...values, ParentId: e.target.value })
                  }
                />
              </Grid>

              {/* CONTROLLER */}
              <Grid size={{ xs: 12 }}>
                <Stack direction="row" justifyContent="space-between">
                  <Typography fontWeight={600}>Controller</Typography>
                  <Button startIcon={<IconPlus size={16} />} onClick={addController}>
                    Tambah Controller
                  </Button>
                </Stack>

                {values.Controllers.map((c: any, cIndex: number) => (
                  <Paper key={cIndex} sx={{ p: 2, mt: 2 }}>
                    <Stack spacing={2}>

                      <TextField
                        size="small"
                        label="ID Controller"
                        fullWidth
                        value={c.IdController}
                        onChange={(e) => {
                          const updated = [...values.Controllers];
                          updated[cIndex].IdController = e.target.value;
                          setValues({ ...values, Controllers: updated });
                        }}
                      />

                      <TextField
                        size="small"
                        label="Nama Controller"
                        fullWidth
                        value={c.NamaController}
                        onChange={(e) => {
                          const updated = [...values.Controllers];
                          updated[cIndex].NamaController = e.target.value;
                          setValues({ ...values, Controllers: updated });
                        }}
                      />

                      {/* URL */}
                      <TextField
                        size="small"
                        label="URL"
                        fullWidth
                        value={c.Url}
                        onChange={(e) => {
                          const updated = [...values.Controllers];
                          updated[cIndex].Url = e.target.value;
                          setValues({ ...values, Controllers: updated });
                        }}
                        placeholder="/hrd/karyawan"
                      />

                      {/* ICON */}
                      <TextField
                        size="small"
                        label="Icon"
                        fullWidth
                        value={c.Icon}
                        onChange={(e) => {
                          const updated = [...values.Controllers];
                          updated[cIndex].Icon = e.target.value;
                          setValues({ ...values, Controllers: updated });
                        }}
                      />

                      <IconButton
                        color="error"
                        onClick={() => removeController(cIndex)}
                      >
                        <IconTrash size={18} />
                      </IconButton>

                      {/* ACTION */}
                      <Box>
                        <Stack direction="row" justifyContent="space-between">
                          <Typography fontSize={13}>Actions</Typography>
                          <Button onClick={() => addAction(cIndex)}>
                            + Action
                          </Button>
                        </Stack>

                        {c.Actions.map((a: any, aIndex: number) => (
                          <Stack key={aIndex} direction="row" spacing={1} mt={1}>
                            <TextField
                              size="small"
                              label="ID Action"
                              value={a.IdAction}
                              onChange={(e) => {
                                const updated = [...values.Controllers];
                                updated[cIndex].Actions[aIndex].IdAction =
                                  e.target.value;
                                setValues({ ...values, Controllers: updated });
                              }}
                            />

                            <TextField
                              size="small"
                              label="Nama Action"
                              fullWidth
                              value={a.NamaAction}
                              onChange={(e) => {
                                const updated = [...values.Controllers];
                                updated[cIndex].Actions[aIndex].NamaAction =
                                  e.target.value;
                                setValues({ ...values, Controllers: updated });
                              }}
                            />

                            <IconButton
                              color="error"
                              onClick={() => removeAction(cIndex, aIndex)}
                            >
                              <IconTrash size={16} />
                            </IconButton>
                          </Stack>
                        ))}
                      </Box>
                    </Stack>
                  </Paper>
                ))}
              </Grid>

              {errors.Controllers && (
                <Typography color="error" fontSize={12}>
                  {errors.Controllers}
                </Typography>
              )}
            </Grid>
          </Box>
        </DialogContent>

        <DialogActions sx={{ px: 3, py: 2 }}>
          <Button onClick={onClose}>Batal</Button>

          <Button
            type="submit"
            variant="contained"
            disabled={isSubmitting}
            startIcon={
              isSubmitting ? (
                <CircularProgress size={18} />
              ) : (
                <IconDeviceFloppy size={16} />
              )
            }
          >
            {isSubmitting ? "Menyimpan..." : "Simpan"}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
};

export default FormMenu;