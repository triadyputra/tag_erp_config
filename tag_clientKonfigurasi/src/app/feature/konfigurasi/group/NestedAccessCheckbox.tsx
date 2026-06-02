import React, { useEffect, useMemo, useState } from 'react';
import {
  Checkbox,
  FormControlLabel,
  Box,
  Typography,
  IconButton,
  Collapse,
  Divider,
} from '@mui/material';
import { ExpandMore, ExpandLess } from '@mui/icons-material';

interface ActionItem {
  IdAction: string;
  NamaAction: string;
}

interface ControllerItem {
  IdController: string;
  Controller: string;
  ActionViewModel: ActionItem[];
}

interface MenuItem {
  IdMenu: string;
  NamaMenu: string;
  IdModul?: string;
  NamaModul?: string;
  ControllerViewModel: ControllerItem[];
}

interface AccessObj {
  IdController: string;
  IdAction: string;
}

interface ModulGroup {
  idModul: string;
  namaModul: string;
  menus: MenuItem[];
}

interface Props {
  data: MenuItem[];
  selected: AccessObj[];
  onChange: (val: AccessObj[]) => void;
  /** Kelompokkan menu per modul (CONFIG, HRD, dll.) */
  groupByModul?: boolean;
}

function collectMenuActions(menu: MenuItem): AccessObj[] {
  const ids: AccessObj[] = [];
  menu.ControllerViewModel.forEach((ctrl) => {
    ctrl.ActionViewModel.forEach((action) => {
      ids.push({
        IdController: ctrl.IdController,
        IdAction: action.IdAction,
      });
    });
  });
  return ids;
}

function collectModulActions(menus: MenuItem[]): AccessObj[] {
  return menus.flatMap(collectMenuActions);
}

function isAllSelected(items: AccessObj[], selected: AccessObj[]) {
  return (
    items.length > 0 &&
    items.every((x) =>
      selected.some(
        (s) => s.IdController === x.IdController && s.IdAction === x.IdAction
      )
    )
  );
}

function isSomeSelected(items: AccessObj[], selected: AccessObj[]) {
  return items.some((x) =>
    selected.some(
      (s) => s.IdController === x.IdController && s.IdAction === x.IdAction
    )
  );
}

function toggleItems(
  items: AccessObj[],
  selected: AccessObj[],
  onChange: (val: AccessObj[]) => void
) {
  const allChecked = isAllSelected(items, selected);
  let updated = [...selected];

  if (allChecked) {
    updated = updated.filter(
      (x) =>
        !items.some(
          (id) =>
            id.IdController === x.IdController && id.IdAction === x.IdAction
        )
    );
  } else {
    items.forEach((id) => {
      if (
        !updated.some(
          (x) =>
            x.IdController === id.IdController && x.IdAction === id.IdAction
        )
      ) {
        updated.push(id);
      }
    });
  }

  onChange(updated);
}

const NestedAccessCheckbox: React.FC<Props> = ({
  data,
  selected,
  onChange,
  groupByModul = true,
}) => {
  const [openModul, setOpenModul] = useState<string[]>([]);
  const [openMenu, setOpenMenu] = useState<string[]>([]);
  const [openController, setOpenController] = useState<string[]>([]);

  const modulGroups = useMemo<ModulGroup[]>(() => {
    if (!groupByModul) {
      return [
        {
          idModul: '_all',
          namaModul: 'Semua Menu',
          menus: data,
        },
      ];
    }

    const map = new Map<string, ModulGroup>();
    data.forEach((menu) => {
      const idModul = menu.IdModul?.trim() || 'LAINNYA';
      const namaModul = menu.NamaModul?.trim() || idModul;
      if (!map.has(idModul)) {
        map.set(idModul, { idModul, namaModul, menus: [] });
      }
      map.get(idModul)!.menus.push(menu);
    });

    return Array.from(map.values());
  }, [data, groupByModul]);

  useEffect(() => {
    if (groupByModul && modulGroups.length > 0) {
      setOpenModul(modulGroups.map((g) => g.idModul));
    }
  }, [modulGroups, groupByModul]);

  const toggleModulCollapse = (modulId: string) => {
    setOpenModul((prev) =>
      prev.includes(modulId)
        ? prev.filter((id) => id !== modulId)
        : [...prev, modulId]
    );
  };

  const toggleMenuCollapse = (menuId: string) => {
    setOpenMenu((prev) =>
      prev.includes(menuId)
        ? prev.filter((id) => id !== menuId)
        : [...prev, menuId]
    );
  };

  const toggleControllerCollapse = (controllerId: string) => {
    setOpenController((prev) =>
      prev.includes(controllerId)
        ? prev.filter((id) => id !== controllerId)
        : [...prev, controllerId]
    );
  };

  const toggleAccess = (controller: string, action: string) => {
    toggleItems(
      [{ IdController: controller, IdAction: action }],
      selected,
      onChange
    );
  };

  const toggleControllerGroup = (controller: ControllerItem) => {
    const ids = controller.ActionViewModel.map((a) => ({
      IdController: controller.IdController,
      IdAction: a.IdAction,
    }));
    toggleItems(ids, selected, onChange);
  };

  const toggleMenuGroup = (menu: MenuItem) => {
    toggleItems(collectMenuActions(menu), selected, onChange);
  };

  const toggleModulGroup = (group: ModulGroup) => {
    toggleItems(collectModulActions(group.menus), selected, onChange);
  };

  const renderMenuBlock = (menu: MenuItem) => {
    const menuActions = collectMenuActions(menu);
    const menuChecked = isAllSelected(menuActions, selected);
    const menuIndeterminate =
      !menuChecked && isSomeSelected(menuActions, selected);

    return (
      <Box key={menu.IdMenu} border={1} borderRadius={2} p={2} mb={2}>
        <Box display="flex" alignItems="center">
          <FormControlLabel
            control={
              <Checkbox
                checked={menuChecked}
                indeterminate={menuIndeterminate}
                onChange={() => toggleMenuGroup(menu)}
              />
            }
            label={<strong>{menu.NamaMenu}</strong>}
          />

          <IconButton
            size="small"
            onClick={() => toggleMenuCollapse(menu.IdMenu)}
          >
            {openMenu.includes(menu.IdMenu) ? (
              <ExpandLess />
            ) : (
              <ExpandMore />
            )}
          </IconButton>
        </Box>

        <Collapse in={openMenu.includes(menu.IdMenu)}>
          <Box ml={3}>
            {menu.ControllerViewModel.map((ctrl) => {
              const ctrlActions = ctrl.ActionViewModel.map((a) => ({
                IdController: ctrl.IdController,
                IdAction: a.IdAction,
              }));

              const ctrlChecked = isAllSelected(ctrlActions, selected);
              const ctrlIndeterminate =
                !ctrlChecked && isSomeSelected(ctrlActions, selected);

              return (
                <Box key={ctrl.IdController} mt={1}>
                  <Box display="flex" alignItems="center">
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={ctrlChecked}
                          indeterminate={ctrlIndeterminate}
                          onChange={() => toggleControllerGroup(ctrl)}
                        />
                      }
                      label={ctrl.Controller}
                    />

                    <IconButton
                      size="small"
                      onClick={() =>
                        toggleControllerCollapse(ctrl.IdController)
                      }
                    >
                      {openController.includes(ctrl.IdController) ? (
                        <ExpandLess />
                      ) : (
                        <ExpandMore />
                      )}
                    </IconButton>
                  </Box>

                  <Collapse in={openController.includes(ctrl.IdController)}>
                    <Box ml={4}>
                      {ctrl.ActionViewModel.map((action) => (
                        <FormControlLabel
                          key={`${ctrl.IdController}-${action.IdAction}`}
                          control={
                            <Checkbox
                              checked={selected.some(
                                (s) =>
                                  s.IdController === ctrl.IdController &&
                                  s.IdAction === action.IdAction
                              )}
                              onChange={() =>
                                toggleAccess(
                                  ctrl.IdController,
                                  action.IdAction
                                )
                              }
                            />
                          }
                          label={action.NamaAction}
                        />
                      ))}
                    </Box>
                  </Collapse>
                </Box>
              );
            })}
          </Box>
        </Collapse>
      </Box>
    );
  };

  return (
    <Box mt={2}>
      <Typography fontWeight="bold" mb={1}>
        Hak Akses
      </Typography>

      {modulGroups.map((group) => {
        const modulActions = collectModulActions(group.menus);
        const modulChecked = isAllSelected(modulActions, selected);
        const modulIndeterminate =
          !modulChecked && isSomeSelected(modulActions, selected);
        const showModulHeader = groupByModul && modulGroups.length > 1;

        return (
          <Box key={group.idModul} mb={3}>
            {showModulHeader && (
              <Box
                sx={(theme) => ({
                  display: 'flex',
                  alignItems: 'center',
                  px: 2,
                  py: 1,
                  mb: 1,
                  borderRadius: 2,
                  bgcolor:
                    theme.palette.mode === 'dark'
                      ? 'grey.900'
                      : 'grey.100',
                  border: `1px solid ${theme.palette.divider}`,
                })}
              >
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={modulChecked}
                      indeterminate={modulIndeterminate}
                      onChange={() => toggleModulGroup(group)}
                    />
                  }
                  label={
                    <Typography fontWeight={700} fontSize={15}>
                      Modul: {group.namaModul}
                      <Typography
                        component="span"
                        variant="caption"
                        color="text.secondary"
                        sx={{ ml: 1 }}
                      >
                        ({group.idModul})
                      </Typography>
                    </Typography>
                  }
                />

                <IconButton
                  size="small"
                  onClick={() => toggleModulCollapse(group.idModul)}
                >
                  {openModul.includes(group.idModul) ? (
                    <ExpandLess />
                  ) : (
                    <ExpandMore />
                  )}
                </IconButton>
              </Box>
            )}

            <Collapse
              in={!showModulHeader || openModul.includes(group.idModul)}
            >
              <Box ml={showModulHeader ? 1 : 0}>
                {group.menus.map(renderMenuBlock)}
              </Box>
            </Collapse>

            {showModulHeader && <Divider sx={{ mt: 1 }} />}
          </Box>
        );
      })}
    </Box>
  );
};

export default NestedAccessCheckbox;
