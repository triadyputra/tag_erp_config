"use client"; // <-- wajib

import React from "react";
import Breadcrumb from "@/app/(DashboardLayout)/layout/shared/breadcrumb/Breadcrumb";
import PageContainer from "@/app/components/container/PageContainer";
import BlankCard from "@/app/components/shared/BlankCard";
import { CardContent } from "@mui/material";
import MenuListComponent from "@/app/feature/konfigurasi/menu";

const BCrumb = [
  {
    to: "/",
    title: "Home",
  },
  {
    title: "Manajemen Menu",
  },
];

const AkunListing = () => {
  return (
        <PageContainer title="Manajemen Menu" description="ini adalah daftar manajemen menu aplikasi">
          <Breadcrumb title="Manajemen Menu" items={BCrumb} />
          <BlankCard>
            <CardContent>
              <MenuListComponent/>
            </CardContent>
          </BlankCard>
        </PageContainer>
  );
}
export default AkunListing;
