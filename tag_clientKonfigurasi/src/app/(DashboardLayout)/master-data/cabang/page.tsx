"use client"; // <-- wajib

import React from "react";
import Breadcrumb from "@/app/(DashboardLayout)/layout/shared/breadcrumb/Breadcrumb";
import PageContainer from "@/app/components/container/PageContainer";
import BlankCard from "@/app/components/shared/BlankCard";
import { CardContent } from "@mui/material";
import CabangListComponent from "@/app/feature/master-data/cabang";

const BCrumb = [
  {
    to: "/",
    title: "Home",
  },
  {
    title: "Daftar Cabang",
  },
];

const AkunListing = () => {
  return (
        <PageContainer title="Daftar Cabang" description="ini adalah daftar cabang">
          <Breadcrumb title="Daftar Cabang" items={BCrumb} />
          <BlankCard>
            <CardContent>
              <CabangListComponent/>
            </CardContent>
          </BlankCard>
        </PageContainer>
  );
}
export default AkunListing;
